using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Models.Exceptions;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.Exceptions.Infrastructure;
using MyCourse.Models.InputModels;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ValueTypes;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
    public class AdoNetCourseService : ICourseService
    {
        private readonly IDatabaseAccessor db;
        private readonly IOptionsMonitor<CoursesOptions> coursesOptions;
        private readonly ILogger<AdoNetCourseService> logger;
        private readonly IImagePersister imagePersister;

        public AdoNetCourseService(IDatabaseAccessor db, IOptionsMonitor<CoursesOptions> coursesOptions, ILogger<AdoNetCourseService> logger, IImagePersister imagePersister)
        {
            this.logger = logger;
            this.coursesOptions = coursesOptions;
            this.db = db;
            this.imagePersister = imagePersister;
        }
        public async Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            logger.LogInformation("Course {id} requested", id);
            FormattableString query = $@"SELECT Id, Title, Description, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Courses WHERE Id={id};
                SELECT Id, Title, Description, Duration FROM Lessons WHERE CourseId={id}";
            DataSet dataSet = await db.QueryAsync(query);
            // Course
            var courseTable = dataSet.Tables[0];
            if(courseTable.Rows.Count != 1) {
                logger.LogWarning("Course {id} not found", id);
                throw new CourseNotFoundException(id);
            }
            var courseDetailViewModel = CourseDetailViewModel.FromDataRow(courseTable.Rows[0]);

            // Course lessons
            var lessonDataTable = dataSet.Tables[1];
            foreach(DataRow lessonRow in lessonDataTable.Rows)
            {
                courseDetailViewModel.Lessons.Add(LessonViewModel.fromDataRow(lessonRow));
            }
            return courseDetailViewModel;
        }
        public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
        {
            var orderBy = model.OrderBy;
            if(orderBy == "CurrentPrice")
            {
                orderBy = "CurrentPrice_Amount";
            }
            string direction = model.Ascending ? "ASC" : "DESC";

            FormattableString query = $@"SELECT Id, Title, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency
                FROM Courses
                WHERE UPPER(Title) LIKE UPPER({"%" + model.Search + "%"})
                ORDER BY {(Sql) orderBy} {(Sql) direction}
                LIMIT {model.Limit}
                OFFSET {model.Offset};

                SELECT COUNT(*)
                FROM Courses
                WHERE UPPER(Title) LIKE UPPER({"%" + model.Search + "%"})";
            DataSet dataSet = await db.QueryAsync(query);
            var dataTable = dataSet.Tables[0];
            var courseList = new List<CourseViewModel>();
            foreach(DataRow courseRow in dataTable.Rows)
            {
                var course = CourseViewModel.FromDataRow(courseRow);
                courseList.Add(course);
            }
            ListViewModel<CourseViewModel> result = new ListViewModel<CourseViewModel>
            {
                Results = courseList,
                TotalCount = Convert.ToInt32(dataSet.Tables[1].Rows[0][0])
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
            string title = inputModel.Title;
            string author = "Mario Rossi";
            FormattableString query = $@"INSERT INTO Courses(Title, Author, ImagePath, CurrentPrice_Currency, CurrentPrice_Amount, FullPrice_Currency, FullPrice_Amount) VALUES ({title}, {author}, '/Courses/default.png', 'EUR', 0, 'EUR', 0);
                SELECT last_insert_rowid();";
            try
            {
                int courseId = await db.QueryScalarAsync<int>(query);
                CourseDetailViewModel course = await GetCourseAsync(courseId);
                return course;
            }
            catch (SqliteException exc) when (exc.SqliteErrorCode == 19)
            {
                throw new CourseTitleUnavailableException(title, exc);
            }
        }

        public async Task<bool> IsTitleAvailableAsync(string title, int id)
        {
            bool titleExists = await db.QueryScalarAsync<bool>($"SELECT COUNT(*) FROM Courses WHERE Title LIKE {title} AND Id<>{id}");
            return !titleExists;
        }

        public async Task<CourseEditInputModel> GetCourseForEditingAsync(int id)
        {
            FormattableString query = $@"SELECT Id, Title, Description, ImagePath, Email, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency, RowVersion FROM Courses WHERE Id = {id}";
            DataSet dataSet = await db.QueryAsync(query);
            var courseTable = dataSet.Tables[0];
            if(courseTable.Rows.Count != 1)
            {
                logger.LogWarning("Course {id} not found", id);
                throw new CourseNotFoundException(id);
            }
            var courseRow = courseTable.Rows[0];
            return CourseEditInputModel.FromDataRow(courseRow);
        }

        public async Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel)
        {
            try
            {
                string imagePath = null;
                if(inputModel.Image != null)
                {
                    imagePath = await imagePersister.SaveCourseImageAsync(inputModel.Id, inputModel.Image);
                }
                var affectedRows = await db.CommandAsync($@"UPDATE Courses SET Title={inputModel.Title}, Description={inputModel.Description}, Email={inputModel.Email}, ImagePath=COALESCE({imagePath}, ImagePath), FullPrice_Amount={inputModel.FullPrice.Amount}, FullPrice_Currency={inputModel.FullPrice.Currency}, CurrentPrice_Amount={inputModel.CurrentPrice.Amount}, CurrentPrice_Currency={inputModel.CurrentPrice.Currency} WHERE Id={inputModel.Id} AND RowVersion={inputModel.RowVersion}");
                if(affectedRows == 0)
                {
                    bool courseExists = await db.QueryScalarAsync<bool>($"SELECT COUNT(*) FROM Courses WHERE Id={inputModel.Id}");
                    if(courseExists)
                    {
                        throw new OptimisticConcurrencyException();
                    }
                    throw new CourseNotFoundException(inputModel.Id);
                }
            }
            catch(ConstraintViolationException exc)
            {
                throw new CourseTitleUnavailableException(inputModel.Title, exc);
            }
            catch(ImagePersistenceException exc)
            {
                throw new CourseImageInvalidException(inputModel.Id, exc);
            }

            CourseDetailViewModel course = await GetCourseAsync(inputModel.Id);
            return course;
        }
    }
}
