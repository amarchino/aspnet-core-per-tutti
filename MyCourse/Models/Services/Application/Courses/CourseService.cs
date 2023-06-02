using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyCourse.Models.Enums;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.ValueTypes;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;
using MyCourse.Models.ViewModels.Lessons;

namespace MyCourse.Models.Services.Application.Courses
{
    public class CourseService : ICourseService
    {
        public Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
        {
            var courseList = new ListViewModel<CourseViewModel>();
            courseList.TotalCount = 20;
            var rand = new Random();
            for (int i = 1; i <= 20; i++)
            {
                var price = Convert.ToDecimal(rand.NextDouble() * 10 + 10);
                var course = new CourseViewModel
                {
                    Id = i,
                    Title = $"Corso {i}",
                    CurrentPrice = new Money(Currency.EUR, price),
                    FullPrice = new Money(Currency.EUR, rand.NextDouble() > 0.5 ? price : price + 1),
                    Author = "Nome cognome",
                    Rating = rand.NextDouble() * 5.0,
                    ImagePath = "~/logo.svg"
                };
                courseList.Results.Add(course);
            }

            return Task.FromResult(courseList);
        }

        public Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            var rand = new Random();
            var price = Convert.ToDecimal(rand.NextDouble() * 10 + 10);
            var course = new CourseDetailViewModel
            {
                Id = id,
                Title = $"Corso {id}",
                CurrentPrice = new Money(Currency.EUR, price),
                FullPrice = new Money(Currency.EUR, rand.NextDouble() > 0.5 ? price : price + 1),
                Author = "Nome cognome",
                Rating = rand.NextDouble() * 5.0,
                ImagePath = "~/logo.svg",
                Description = $"Descrizione {id}",
                Lessons = new List<LessonViewModel>()
            };

            for (var i = 1; i <= 5; i++)
            {
                var lesson = new LessonViewModel
                {
                    Title = $"Lezione {i}",
                    Duration = TimeSpan.FromSeconds(rand.Next(40, 90))
                };
                course.Lessons.Add(lesson);
            }

            return Task.FromResult(course);
        }

        public Task<List<CourseViewModel>> getBestRatingCoursesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<CourseViewModel>> getMostRecentCoursesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel inputModel)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsTitleAvailableAsync(string title, int id)
        {
            throw new NotImplementedException();
        }

        public Task<CourseEditInputModel> GetCourseForEditingAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCourseAsync(CourseDeleteInputModel inputModel)
        {
            throw new NotImplementedException();
        }

        public Task SendQuestionToCourseAuthorAsync(int id, string question)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetCourseAuthorIdAsync(int courseId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCourseCountByAuthorIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task SubscribeCourseAsync(CourseSubscribeInputModel inputModel)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsCourseSubscribedAsync(int courseId, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
