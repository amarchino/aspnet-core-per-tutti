using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Customizations.Authorization;
using MyCourse.Models.Enums;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.InputModels.Lessons;
using MyCourse.Models.ViewModels.Lessons;

namespace MyCourse.Controllers;

public class LessonsController : Controller
{
    private readonly ICachedLessonService lessonService;
    public LessonsController(ICachedLessonService lessonService)
    {
        this.lessonService = lessonService;
    }

    [Authorize(Policy = nameof(Policy.CourseAuthor) + "," + nameof(Policy.CourseSubscriber))]
    public async Task<IActionResult> Detail(int id)
    {
        LessonDetailViewModel viewModel = await lessonService.GetLessonAsync(id);
        ViewData["Title"] = viewModel.Title;
        return View(viewModel);
    }

    [AuthorizeRole(Role.Teacher)]
    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    public IActionResult Create(int id)
    {
        ViewData["Title"] = "Nuova lesione";
        LessonCreateInputModel inputModel = new();
        inputModel.CourseId = id;
        return View(inputModel);
    }

    [HttpPost]
    [AuthorizeRole(Role.Teacher)]
    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    public async Task<IActionResult> Create(LessonCreateInputModel inputModel)
    {
        if (ModelState.IsValid)
        {
            LessonDetailViewModel lesson = await lessonService.CreateLessonAsync(inputModel);
            TempData["ConfirmationMessage"] = "Ok! La lezione è stata creata, aggiungi anche gli altri dati";
            return RedirectToAction(nameof(Edit), new { id = lesson.Id });
        }

        ViewData["Title"] = "Nuova lezione";
        return View(inputModel);
    }

    [AuthorizeRole(Role.Teacher)]
    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Modifica lezione";
        LessonEditInputModel inputModel = await lessonService.GetLessonForEditingAsync(id);
        return View(inputModel);
    }

    [HttpPost]
    [AuthorizeRole(Role.Teacher)]
    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    public async Task<IActionResult> Edit(LessonEditInputModel inputModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                LessonDetailViewModel lesson = await lessonService.EditLessonAsync(inputModel);
                TempData["ConfirmationMessage"] = "I dati sono stati salvati con successo";
                return RedirectToAction(nameof(Detail), new { id = inputModel.Id });
            }
            catch (OptimisticConcurrencyException)
            {
                ModelState.AddModelError("", "Spiacenti, il salvataggio non è andato a buon fine perché nel frattempo un altro utente ha aggiornato la lezione. Ti preghiamo di aggiornare la pagina e ripetere le modifiche.");
            }
        }

        ViewData["Title"] = "Modifica lezione";
        return View(inputModel);
    }

    [HttpPost]
    [AuthorizeRole(Role.Teacher)]
    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    public async Task<IActionResult> Delete(LessonDeleteInputModel inputModel)
    {
        await lessonService.DeleteLessonAsync(inputModel);
        TempData["ConfirmationMessage"] = "La lezione è stata eliminata";
        return RedirectToAction(nameof(CoursesController.Detail), "Courses", new { id = inputModel.CourseId });
    }
}
