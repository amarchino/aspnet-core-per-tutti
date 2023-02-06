using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyCourse.Models.Entities;
using MyCourse.Models.Exceptions;
using MyCourse.Models.InputModels;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
    public class EfCoreCourseService : ICourseService
    {
        private readonly MyCourseDbContext dbContext;
        private readonly IOptionsMonitor<CoursesOptions> coursesOptions;
        private readonly IImagePersister imagePersister;

        public EfCoreCourseService(MyCourseDbContext dbContext, IOptionsMonitor<CoursesOptions> coursesOptions, IImagePersister imagePersister)
        {
            this.coursesOptions = coursesOptions;
            this.dbContext = dbContext;
            this.imagePersister = imagePersister;
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

        public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
        {
            IQueryable<Course> baseQuery = dbContext.Courses;
            switch(model.OrderBy)
            {
                case "Id":
                    baseQuery = model.Ascending ? baseQuery.OrderBy(course => course.Id) : baseQuery.OrderByDescending(course => course.Id);
                    break;
                case "Title":
                    baseQuery = model.Ascending ? baseQuery.OrderBy(course => course.Title) : baseQuery.OrderByDescending(course => course.Title);
                    break;
                case "Rating":
                    baseQuery = model.Ascending ? baseQuery.OrderBy(course => course.Rating) : baseQuery.OrderByDescending(course => course.Rating);
                    break;
                case "CurrentPrice":
                    baseQuery = model.Ascending ? baseQuery.OrderBy(course => course.CurrentPrice.Amount) : baseQuery.OrderByDescending(course => course.CurrentPrice.Amount);
                    break;
            }

            IQueryable<CourseViewModel> queryLinq = baseQuery
                .AsNoTracking()
                .Where(course => course.Title.ToUpper().Contains(model.Search.ToUpper()))
                .Select(course => new CourseViewModel{
                    Id = course.Id,
                    Title = course.Title,
                    ImagePath = course.ImagePath,
                    Author = course.Author,
                    Rating = course.Rating,
                    CurrentPrice = course.CurrentPrice,
                    FullPrice = course.FullPrice
                })
                ;
            List<CourseViewModel> courses = await queryLinq
                .Skip(model.Offset)
                .Take(model.Limit)
                .ToListAsync<CourseViewModel>();
            int TotalCount = await queryLinq.CountAsync();
            ListViewModel<CourseViewModel> result = new ListViewModel<CourseViewModel>
            {
                Results = courses,
                TotalCount = TotalCount
            };
            return result;
        }

        public async Task<List<CourseViewModel>> getBestRatingCoursesAsync()
        {
            return (await GetCoursesAsync(new CourseListInputModel(
                search: "",
                page: 1,
                orderby: "Rating",
                ascending: false,
                limit: coursesOptions.CurrentValue.InHome,
                orderOptions: coursesOptions.CurrentValue.Order
            ))).Results;
        }

        public async Task<List<CourseViewModel>> getMostRecentCoursesAsync()
        {
            return (await GetCoursesAsync(new CourseListInputModel(
                search: "",
                page: 1,
                orderby: "Id",
                ascending: false,
                limit: coursesOptions.CurrentValue.InHome,
                orderOptions: coursesOptions.CurrentValue.Order
            ))).Results;
        }

        public async Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel inputModel)
        {
            var entity = new Course(inputModel.Title, "Mario Rossi");
            dbContext.Courses.Add(entity);

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exc) when ((exc.InnerException as SqliteException)?.SqliteErrorCode == 19)
            {
                throw new CourseTitleUnavailableException(inputModel.Title, exc);
            }

            return CourseDetailViewModel.FromEntity(entity);
        }

        public async Task<bool> IsTitleAvailableAsync(string title, int id)
        {
            bool titleExists = await dbContext.Courses
                .Where(course => course.Id != id)
                .AnyAsync(course => EF.Functions.Like(course.Title, title));
            return !titleExists;
        }

        public async Task<CourseEditInputModel> GetCourseForEditingAsync(int id)
        {
            CourseEditInputModel viewModel = await dbContext.Courses
                .AsNoTracking()
                .Where(course => course.Id == id)
                .Select(course => new CourseEditInputModel{
                    Id = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    ImagePath = course.ImagePath,
                    Email = course.Email,
                    FullPrice = course.FullPrice,
                    CurrentPrice = course.CurrentPrice,
                }).SingleAsync<CourseEditInputModel>();

            return viewModel;
        }

        public async Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel)
        {
            Course course = await dbContext.Courses.FindAsync(inputModel.Id);
            course.ChangeTitle(inputModel.Title);
            course.ChangePrices(inputModel.FullPrice, inputModel.CurrentPrice);
            course.changeDescription(inputModel.Description);
            course.changeEmail(inputModel.Email);
            // dbContext.Courses.Update(entity);

            string imagePath = await imagePersister.SaveCourseImageAsync(inputModel.Id, inputModel.Image);
            course.ChangeImagePath(imagePath);

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exc) when ((exc.InnerException as SqliteException)?.SqliteErrorCode == 19)
            {
                throw new CourseTitleUnavailableException(inputModel.Title, exc);
            }

            return CourseDetailViewModel.FromEntity(course);
        }
    }
}
