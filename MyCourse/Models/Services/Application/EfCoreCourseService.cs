using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
    public class EfCoreCourseService : ICourseService
    {
        private readonly MyCourseDbContext dbContext;
        private readonly IOptionsMonitor<CoursesOptions> coursesOptions;

        public EfCoreCourseService(MyCourseDbContext dbContext, IOptionsMonitor<CoursesOptions> coursesOptions)
        {
            this.coursesOptions = coursesOptions;
            this.dbContext = dbContext;
        }

        public async Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            CourseDetailViewModel viewModel = await dbContext.Courses
                .AsNoTracking()
                .Where(course => course.Id == id)
                .Select(course => new CourseDetailViewModel{
                    Id = course.Id,
                    Title = course.Title,
                    ImagePath = course.ImagePath,
                    Author = course.Author,
                    Rating = course.Rating,
                    CurrentPrice = course.CurrentPrice,
                    FullPrice = course.FullPrice,
                    Description = course.Description,
                    Lessons = course.Lessons.Select(lesson => new LessonViewModel{
                        Id = lesson.Id,
                        Title = lesson.Title,
                        Description = lesson.Description,
                        Duration = lesson.Duration
                    }).ToList<LessonViewModel>()
                }).SingleAsync<CourseDetailViewModel>();

            return viewModel;
        }

        public async Task<List<CourseViewModel>> GetCoursesAsync(string search, int page)
        {
            search = search ?? "";
            page = Math.Max(1, page);
            int limit = coursesOptions.CurrentValue.PerPage;
            int offset = (page - 1) * 10;

            IQueryable<CourseViewModel> queryLinq = dbContext.Courses
                .AsNoTracking()
                .Where(course => course.Title.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                .Select(course => new CourseViewModel{
                    Id = course.Id,
                    Title = course.Title,
                    ImagePath = course.ImagePath,
                    Author = course.Author,
                    Rating = course.Rating,
                    CurrentPrice = course.CurrentPrice,
                    FullPrice = course.FullPrice
                })
                .Skip(offset)
                .Take(limit);
            List<CourseViewModel> courses = await queryLinq.ToListAsync<CourseViewModel>();
            return courses;
        }
    }
}
