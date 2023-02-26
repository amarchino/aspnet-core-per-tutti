using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyCourse.Models.Entities;
using MyCourse.Models.Exceptions;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;
using MyCourse.Models.ViewModels.Lessons;

namespace MyCourse.Models.Services.Application.Courses
{
    public class EfCoreCourseService : ICourseService
    {
        private readonly MyCourseDbContext dbContext;
        private readonly IOptionsMonitor<CoursesOptions> coursesOptions;
        private readonly IImagePersister imagePersister;
        private readonly IHttpContextAccessor httpContextAccessor;

        public EfCoreCourseService(
            MyCourseDbContext dbContext,
            IOptionsMonitor<CoursesOptions> coursesOptions,
            IImagePersister imagePersister,
            IHttpContextAccessor httpContextAccessor)
        {
            this.coursesOptions = coursesOptions;
            this.dbContext = dbContext;
            this.imagePersister = imagePersister;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            CourseDetailViewModel viewModel = await dbContext.Courses
                .AsNoTracking()
                .Where(course => course.Id == id)
                .Select(course => new CourseDetailViewModel
                {
                    Id = course.Id,
                    Title = course.Title,
                    ImagePath = course.ImagePath,
                    Author = course.Author,
                    Rating = course.Rating,
                    CurrentPrice = course.CurrentPrice,
                    FullPrice = course.FullPrice,
                    Description = course.Description,
                    Lessons = course.Lessons.Select(lesson => LessonViewModel.FromEntity(lesson)).ToList()
                }).SingleAsync();

            return viewModel;
        }

        public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
        {
            IQueryable<Course> baseQuery = dbContext.Courses;
            switch (model.OrderBy)
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
                .Select(course => new CourseViewModel
                {
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
                .ToListAsync();
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
            string author;
            string authorId;
            try
            {
                author = httpContextAccessor.HttpContext.User.FindFirst("FullName").Value;
                authorId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
            catch(NullReferenceException)
            {
                throw new UserUnknownException();
            }
            var entity = new Course(inputModel.Title, author, authorId);
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
                .Select(course => new CourseEditInputModel
                {
                    Id = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    ImagePath = course.ImagePath,
                    Email = course.Email,
                    FullPrice = course.FullPrice,
                    CurrentPrice = course.CurrentPrice,
                    RowVersion = course.RowVersion
                }).SingleAsync();

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

            dbContext.Entry(course).Property(course => course.RowVersion).OriginalValue = inputModel.RowVersion;

            try
            {
                if (inputModel.Image != null)
                {
                    string imagePath = await imagePersister.SaveCourseImageAsync(inputModel.Id, inputModel.Image);
                    course.ChangeImagePath(imagePath);
                }
            }
            catch (Exception exc)
            {
                throw new CourseImageInvalidException(inputModel.Id, exc);
            }

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exc) when ((exc.InnerException as SqliteException)?.SqliteErrorCode == 19)
            {
                throw new CourseTitleUnavailableException(inputModel.Title, exc);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new OptimisticConcurrencyException();
            }

            return CourseDetailViewModel.FromEntity(course);
        }

        public async Task DeleteCourseAsync(CourseDeleteInputModel inputModel)
        {
            Course course = await dbContext.Courses.FindAsync(inputModel.Id);
            if(course == null)
            {
                throw new CourseNotFoundException(inputModel.Id);
            }
            course.ChangeStatus(Enums.CourseStatus.Deleted);
            await dbContext.SaveChangesAsync();
        }
    }
}
