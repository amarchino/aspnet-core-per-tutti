using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Services.Application;
using MyCourse.Models.ViewModels;

namespace MyCourse.Controllers
{
    public class HomeController : Controller
    {

        // [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [ResponseCache(CacheProfileName = "Home")]
        public async Task<IActionResult> Index([FromServices] ICachedCourseService courseService)
        {
            ViewData["Title"] = "MyCourse";

            List<CourseViewModel> bestRatingCourses = await courseService.getBestRatingCoursesAsync();
            List<CourseViewModel> mostRecentCourses = await courseService.getMostRecentCoursesAsync();

            HomeViewModel viewModel = new HomeViewModel
            {
                BestRatingCourses = bestRatingCourses,
                MostRecentCourses = mostRecentCourses
            };
            return View(viewModel);
        }
    }
}
