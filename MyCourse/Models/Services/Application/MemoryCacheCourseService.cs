using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MyCourse.Models.Options;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
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
            return memoryCache.GetOrCreateAsync($"Course{id}", cacheEntry => {
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(coursesOptions.CurrentValue.CacheDuration));
                return courseService.GetCourseAsync(id);
            });
        }

        public Task<List<CourseViewModel>> GetCoursesAsync()
        {
            return memoryCache.GetOrCreateAsync($"Courses", cacheEntry => {
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(coursesOptions.CurrentValue.CacheDuration));
                return courseService.GetCoursesAsync();
            });
        }
    }
}
