using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.Entities;
using MyCourse.Models.Enums;
using MyCourse.Models.ValueTypes;

namespace MyCourse.Models.ViewModels.Lessons
{
    public class LessonDetailViewModel : LessonViewModel
    {
        public int CourseId { get; set; }
        public string Description { get; set; }

        public static LessonDetailViewModel FromDataRow(DataRow lessonRow)
        {
            return new LessonDetailViewModel
            {
                Id = Convert.ToInt32(lessonRow["Id"]),
                CourseId = Convert.ToInt32(lessonRow["CourseId"]),
                Description = Convert.ToString(lessonRow["Description"]),
                Title = Convert.ToString(lessonRow["Title"]),
                Duration = TimeSpan.Parse(Convert.ToString(lessonRow["Duration"]))
            };
        }

        public static new LessonDetailViewModel FromEntity(Lesson lesson)
        {
            return new LessonDetailViewModel
            {
                Id = lesson.Id,
                CourseId = lesson.CourseId,
                Description = lesson.Description,
                Title = lesson.Title,
                Duration = lesson.Duration
            };
        }
    }
}
