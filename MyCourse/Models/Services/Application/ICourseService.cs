using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.InputModels;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
    public interface ICourseService
    {
        Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model);
        Task<CourseDetailViewModel> GetCourseAsync(int id);
        Task<List<CourseViewModel>> getBestRatingCoursesAsync();
        Task<List<CourseViewModel>> getMostRecentCoursesAsync();
        Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel inputModel);
        Task<bool> IsTitleAvailableAsync(string title);
    }
}
