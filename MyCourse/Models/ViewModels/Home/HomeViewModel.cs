using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.ViewModels.Courses;

namespace MyCourse.Models.ViewModels.Home
{
    public class HomeViewModel
    {
        public List<CourseViewModel> BestRatingCourses { get; set; }
        public List<CourseViewModel> MostRecentCourses { get; set; }
    }
}
