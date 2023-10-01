using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.ViewModels.Courses;
using MyCourse.Models.ViewModels.Home;

namespace MyCourse.Controllers;

public class HomeController : Controller
{

    // [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
    // [ResponseCache(CacheProfileName = "Home")]
    [AllowAnonymous]
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
