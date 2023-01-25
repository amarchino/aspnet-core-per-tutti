using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Services.Application;
using MyCourse.Models.ViewModels;

namespace MyCourse.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICourseService courseService;
        public CoursesController(ICourseService courseService)
        {
            this.courseService = courseService;
        }
        public async Task<IActionResult> Index(string search = null, int page = 1, string orderby = "price", bool ascending = true)
        {
            ViewData["Title"] = "Catalogo dei corsi";
            List<CourseViewModel> courses = await courseService.GetCoursesAsync(search, page);
            return View(courses);
        }

        public async Task<IActionResult> Detail(int id)
        {
            CourseDetailViewModel course = await courseService.GetCourseAsync(id);
            ViewData["Title"] = course.Title;
            return View(course);
        }
    }
}
