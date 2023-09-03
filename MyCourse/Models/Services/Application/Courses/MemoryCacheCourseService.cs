using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;

namespace MyCourse.Models.Services.Application.Courses
{
    public class MemoryCacheCourseService : ICachedCourseService
    {
        private readonly ICourseService courseService;
        private readonly IMemoryCache memoryCache;
        private readonly IOptionsMonitor<CoursesOptions> coursesOptions;
        public MemoryCacheCourseService(ICourseService courseService, IMemoryCache memoryCache, IOptionsMonitor<CoursesOptions> coursesOptions)
        {
            this.coursesOptions = coursesOptions;
            this.memoryCache = memoryCache;
            this.courseService = courseService;
        }

        public Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            return memoryCache.GetOrCreateAsync($"Course{id}", cacheEntry =>
            {
                cacheEntry.SetSize(1);
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(coursesOptions.CurrentValue.CacheDuration));
                return courseService.GetCourseAsync(id);
            });
        }

        public Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
        {
            // Metto in cache i risultati solo per le prime 5 pagine del catalogo, che reputo essere le più visitate dagli utenti, e che perciò mi permettono di avere il maggior beneficio dalla cache.
            // E inoltre, metto in cache i risultati solo se l'utente non ha cercato nulla. In questo modo riduco drasticamente il consumo di memoria RAM
            bool canCache = model.Page <= 5 && string.IsNullOrEmpty(model.Search);
            // Se canCache è true, sfrutto il meccanismo di caching
            if (canCache)
            {
                return memoryCache.GetOrCreateAsync($"Courses{model.Page}-{model.OrderBy}-{model.Ascending}", cacheEntry =>
                {
                    cacheEntry.SetSize(2);
                    cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(coursesOptions.CurrentValue.CacheDuration));
                    return courseService.GetCoursesAsync(model);
                });
            }
            // Altrimenti uso il servizio applicativo sosttostante, che recupererò sempre i valori dal database
            return courseService.GetCoursesAsync(model);
        }

        public Task<List<CourseViewModel>> getBestRatingCoursesAsync()
        {
            return memoryCache.GetOrCreateAsync($"BestRatingCourses", cacheEntry =>
            {
                cacheEntry.SetSize(1);
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(coursesOptions.CurrentValue.CacheDuration));
                return courseService.getBestRatingCoursesAsync();
            });
        }

        public Task<List<CourseViewModel>> getMostRecentCoursesAsync()
        {
            return memoryCache.GetOrCreateAsync($"MostRecentCourses", cacheEntry =>
            {
                cacheEntry.SetSize(1);
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(coursesOptions.CurrentValue.CacheDuration));
                return courseService.getMostRecentCoursesAsync();
            });
        }

        public Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel inputModel)
        {
            return courseService.CreateCourseAsync(inputModel);
        }

        public Task<bool> IsTitleAvailableAsync(string title, int id)
        {
            return courseService.IsTitleAvailableAsync(title, id);
        }

        public Task<CourseEditInputModel> GetCourseForEditingAsync(int id)
        {
            return courseService.GetCourseForEditingAsync(id);
        }

        public async Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel)
        {
            CourseDetailViewModel viewModel = await courseService.EditCourseAsync(inputModel);
            memoryCache.Remove($"Course{inputModel.Id}");
            return viewModel;
        }

        public Task DeleteCourseAsync(CourseDeleteInputModel inputModel)
        {
            return courseService.DeleteCourseAsync(inputModel);
        }

        public Task SendQuestionToCourseAuthorAsync(int id, string question)
        {
            return courseService.SendQuestionToCourseAuthorAsync(id, question);
        }

        public Task<string> GetCourseAuthorIdAsync(int courseId)
        {
            return memoryCache.GetOrCreateAsync($"CourseAuthor{courseId}", cacheEntry =>
            {
                cacheEntry.SetSize(1);
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(coursesOptions.CurrentValue.CacheDuration));
                return courseService.GetCourseAuthorIdAsync(courseId);
            });
        }

        public Task<int> GetCourseCountByAuthorIdAsync(string userId)
        {
            return courseService.GetCourseCountByAuthorIdAsync(userId);
        }

        public Task SubscribeCourseAsync(CourseSubscribeInputModel inputModel)
        {
            return courseService.SubscribeCourseAsync(inputModel);
        }

        public Task<bool> IsCourseSubscribedAsync(int courseId, string userId)
        {
            return courseService.IsCourseSubscribedAsync(courseId, userId);
        }

        public Task<string> GetPaymentUrlAsync(int id)
        {
            return courseService.GetPaymentUrlAsync(id);
        }

        public Task<CourseSubscribeInputModel> CapturePaymentAsync(int id, string token)
        {
            return courseService.CapturePaymentAsync(id, token);
        }
    }
}
