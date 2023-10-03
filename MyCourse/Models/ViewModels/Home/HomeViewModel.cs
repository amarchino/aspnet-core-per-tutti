using MyCourse.Models.ViewModels.Courses;

namespace MyCourse.Models.ViewModels.Home;
public class HomeViewModel
{
    public List<CourseViewModel> BestRatingCourses { get; set; } = new List<CourseViewModel>();
    public List<CourseViewModel> MostRecentCourses { get; set; } = new List<CourseViewModel>();
}
