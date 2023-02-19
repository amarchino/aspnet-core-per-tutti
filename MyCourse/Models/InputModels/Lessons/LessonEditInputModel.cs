using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Controllers;
using MyCourse.Models.Entities;
using MyCourse.Models.Enums;
using MyCourse.Models.ValueTypes;

namespace MyCourse.Models.InputModels.Lessons
{
    public class LessonEditInputModel
    {
        [Required]
        public int Id { get; set; }
        public int CourseId { get; set; }
        [Required(ErrorMessage = "Il titolo è obbligatorio"),
        MinLength(10, ErrorMessage = "Il titolo dev'essere di almeno {1} caratteri"),
        MaxLength(100, ErrorMessage = "Il titolo dev'essere di al massimo {1} caratteri"),
        RegularExpression(@"^[0-9A-z\u00C0-\u00ff\s\.']+$", ErrorMessage = "Titolo non valido"),
        Display(Name = "Titolo")]
        public string Title { get; set; }
        [MinLength(10, ErrorMessage = "La descrizione dev'essere di almeno {1} caratteri"),
        MaxLength(1000, ErrorMessage = "La descrizione dev'essere di massimo {1} caratteri"),
        Display(Name = "Descrizione")]
        public string Description { get; set; }
        [Display(Name = "Durata stimata"),
        Required(ErrorMessage = "La durata è richiesta")]
        public TimeSpan Duration { get; set; }
        [Display(Name = "Ordine"),
        Required(ErrorMessage = "L'ordine è richiesto")]
        public int Order { get; set; }
        public string RowVersion { get; set; }

        public static LessonEditInputModel FromDataRow(DataRow lessonRow)
        {
            return new LessonEditInputModel
            {
                Id = Convert.ToInt32(lessonRow["Id"]),
                CourseId = Convert.ToInt32(lessonRow["CourseId"]),
                Title = Convert.ToString(lessonRow["Title"]),
                Description = Convert.ToString(lessonRow["Description"]),
                Duration = TimeSpan.Parse(Convert.ToString(lessonRow["Duration"])),
                Order = Convert.ToInt32(lessonRow["Order"]),
                RowVersion = Convert.ToString(lessonRow["RowVersion"])
            };
        }

        public static LessonEditInputModel FromEntity(Lesson lesson)
        {
            return new LessonEditInputModel
            {
                Id = lesson.Id,
                CourseId = lesson.CourseId,
                Title = lesson.Title,
                Description = lesson.Description,
                Duration = lesson.Duration,
                Order = lesson.Order,
                RowVersion = lesson.RowVersion
            };
        }
    }
}
