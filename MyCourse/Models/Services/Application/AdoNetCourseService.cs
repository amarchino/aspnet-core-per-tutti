using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Models.Exceptions;
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

        public AdoNetCourseService(IDatabaseAccessor db, IOptionsMonitor<CoursesOptions> coursesOptions, ILogger<AdoNetCourseService> logger)
        {
            this.logger = logger;
            this.coursesOptions = coursesOptions;
            this.db = db;
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
                OFFSET {model.Offset},

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
    }
}
