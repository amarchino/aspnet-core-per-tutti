using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyCourse.Customizations.Authorization;
using MyCourse.Models.Enums;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;

namespace MyCourse.Controllers;

public class CoursesController : Controller
{
    private readonly ICourseService courseService;

    public CoursesController(ICachedCourseService courseService)
    {
        this.courseService = courseService;
    }
    [AllowAnonymous]
    public async Task<IActionResult> Index(CourseListInputModel model)
    {
        ViewData["Title"] = "Catalogo dei corsi";
        ListViewModel<CourseViewModel> courses = await courseService.GetCoursesAsync(model);

        CourseListViewModel viewModel = new CourseListViewModel
        {
            Courses = courses,
            Input = model
        };
        return View(viewModel);
    }

    public async Task<IActionResult> Pay(int id)
    {
        string paymentUrl = await courseService.GetPaymentUrlAsync(id);
        return Redirect(paymentUrl);
    }

    public async Task<IActionResult> Subscribe(int id, string token)
    {
        CourseSubscribeInputModel inputModel = await courseService.CapturePaymentAsync(id, token);
        await courseService.SubscribeCourseAsync(inputModel);
        TempData["ConfirmationMessage"] = "Grazie per esserti iscritto, guarda subito la prima lezione!";
        return RedirectToAction(nameof(Detail), new { id = id });
    }

    [AllowAnonymous]
    public async Task<IActionResult> Detail(int id)
    {
        CourseDetailViewModel course = await courseService.GetCourseAsync(id);
        ViewData["Title"] = course.Title;
        return View(course);
    }

    [AuthorizeRole(Role.Teacher, Role.Administrator)]
    public IActionResult Create()
    {
        ViewData["Title"] = "Nuovo corso";
        CourseCreateInputModel inputModel = new ();
        return View(inputModel);
    }

    [HttpPost]
    [AuthorizeRole(Role.Teacher, Role.Administrator)]
    public async Task<IActionResult> Create(
        CourseCreateInputModel inputModel,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IEmailClient emailClient,
        [FromServices] IOptionsMonitor<UsersOptions> usersOptions)
    {
        if(ModelState.IsValid)
        {
            try
            {
                CourseDetailViewModel course = await courseService.CreateCourseAsync(inputModel);
                AuthorizationResult result = await authorizationService.AuthorizeAsync(User, nameof(Policy.CourseLimit));
                if(!result.Succeeded)
                {
                    await emailClient.SendEmailAsync(
                        usersOptions.CurrentValue.NotificationEmailRecipient,
                        "Avviso superamento soglia",
                        $"Il docente {User.Identity?.Name} ha craeto molti corsi: verifica che riesca a gestirli tutti.");
                }

                TempData["ConfirmationMessage"] = "Ok! Il tuo corso è stato creato, ora perché non inserisci anche gli altri dati?";
                return RedirectToAction(nameof(Edit), new { id = course.Id });
            }
            catch (CourseTitleUnavailableException)
            {
                ModelState.AddModelError(nameof(CourseDetailViewModel.Title), "Questo titolo già esiste");
            }
        }

        ViewData["Title"] = "Nuovo corso";
        return View(inputModel);
    }

    [AuthorizeRole(Role.Teacher, Role.Administrator)]
    public async Task<IActionResult> IsTitleAvailable(string title, int id = 0)
    {
        bool result = await courseService.IsTitleAvailableAsync(title, id);
        return Json(result);
    }

    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    [AuthorizeRole(Role.Teacher, Role.Administrator)]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Modifica corso";
        CourseEditInputModel inputModel = await courseService.GetCourseForEditingAsync(id);
        return View(inputModel);
    }

    [HttpPost]
    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    [AuthorizeRole(Role.Teacher, Role.Administrator)]
    public async Task<IActionResult> Edit(CourseEditInputModel inputModel)
    {
        if(ModelState.IsValid)
        {
            try
            {
                CourseDetailViewModel course = await courseService.EditCourseAsync(inputModel);
                TempData["ConfirmationMessage"] = "I dati sono stati salvati con successo";
                return RedirectToAction(nameof(Detail), new { id = inputModel.Id });
            }
            catch(CourseTitleUnavailableException)
            {
                ModelState.AddModelError(nameof(CourseEditInputModel.Title), "Questo titolo già esiste");
            }
            catch(CourseImageInvalidException)
            {
                ModelState.AddModelError(nameof(CourseEditInputModel.Image), "L'immagine selezionata non è valida");
            }
            catch (OptimisticConcurrencyException)
            {
                ModelState.AddModelError("", "Spiacenti, il salvataggio non è andato a buon fine perché nel frattempo un altro utente ha aggiornato il corso. Ti preghiamo di aggiornare la pagina e ripetere le modifiche.");
            }
        }

        ViewData["Title"] = "Modifica corso";
        return View(inputModel);
    }

    [HttpPost]
    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    [AuthorizeRole(Role.Teacher, Role.Administrator)]
    public async Task<IActionResult> Delete(CourseDeleteInputModel inputModel)
    {
        await courseService.DeleteCourseAsync(inputModel);
        TempData["ConfirmationMessage"] = "Il corseo è stato eliminato ma potrebbe continuare a comparire negli elenchi per un breve periodo, finché la cache non viene aggiornata.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = nameof(Policy.CourseSubscriber))]
    public async Task<IActionResult> Vote(int id)
    {
        CourseVoteInputModel inputModel = new()
        {
            Id = id,
            Vote = await courseService.GetCourseVoteAsync(id) ?? 0
        };
        return View(inputModel);
    }

    [HttpPost]
    [Authorize(Policy = nameof(Policy.CourseSubscriber))]
    public async Task<IActionResult> Vote(CourseVoteInputModel inputModel)
    {
        await courseService.VoteCourseAsync(inputModel);
        TempData["ConfirmationMessage"] = "Grazie per aver votato.";
        return RedirectToAction(nameof(Detail), new { id = inputModel.Id });
    }
}
