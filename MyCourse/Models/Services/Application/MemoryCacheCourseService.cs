using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MyCourse.Models.InputModels;
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
                cacheEntry.SetSize(1);
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(coursesOptions.CurrentValue.CacheDuration));
                return courseService.GetCourseAsync(id);
            });
        }

        public Task<List<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
        {
            // Metto in cache i risultati solo per le prime 5 pagine del catalogo, che reputo essere le più visitate dagli utenti, e che perciò mi permettono di avere il maggior beneficio dalla cache.
            // E inoltre, metto in cache i risultati solo se l'utente non ha cercato nulla. In questo modo riduco drasticamente il consumo di memoria RAM
            bool canCache = model.Page <= 5 && string.IsNullOrEmpty(model.Search);
            // Se canCache è true, sfrutto il meccanismo di caching
            if(canCache)
            {
                return memoryCache.GetOrCreateAsync($"Courses{model.Page}-{model.OrderBy}-{model.Ascending}", cacheEntry => {
                    cacheEntry.SetSize(2);
                    cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(coursesOptions.CurrentValue.CacheDuration));
                    return courseService.GetCoursesAsync(model);
                });
            }
            // Altrimenti uso il servizio applicativo sosttostante, che recupererò sempre i valori dal database
            return courseService.GetCoursesAsync(model);
        }
    }
}
