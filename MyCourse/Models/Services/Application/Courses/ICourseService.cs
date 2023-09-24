using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;

namespace MyCourse.Models.Services.Application.Courses
{
    public interface ICourseService
    {
        Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model);
        Task<CourseDetailViewModel> GetCourseAsync(int id);
        Task<List<CourseViewModel>> getBestRatingCoursesAsync();
        Task<List<CourseViewModel>> getMostRecentCoursesAsync();
        Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel inputModel);
        Task<bool> IsTitleAvailableAsync(string title, int id);
        Task<CourseEditInputModel> GetCourseForEditingAsync(int id);
        Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel);
        Task DeleteCourseAsync(CourseDeleteInputModel inputModel);
        Task SendQuestionToCourseAuthorAsync(int id, string question);
        Task<string> GetCourseAuthorIdAsync(int courseId);
        Task<int> GetCourseCountByAuthorIdAsync(string userId);
        Task SubscribeCourseAsync(CourseSubscribeInputModel inputModel);
        Task<bool> IsCourseSubscribedAsync(int courseId, string userId);
        Task<string> GetPaymentUrlAsync(int id);
        Task<CourseSubscribeInputModel> CapturePaymentAsync(int id, string token);
        Task<int?> GetCourseVoteAsync(int id);
        Task VoteCourseAsync(CourseVoteInputModel inputModel);
    }
}
