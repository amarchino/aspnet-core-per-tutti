using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Controllers;
using MyCourse.Customizations.ValidationAttributes;
using MyCourse.Models.Entities;

namespace MyCourse.Models.InputModels.Lessons
{
    public class LessonCreateInputModel
    {
        [Required]
        public int CourseId { get; set; }
        [Required(ErrorMessage = "Il titolo è obbligatorio"),
        MinLength(10, ErrorMessage = "Il titolo dev'essere di almeno {1} caratteri"),
        MaxLength(100, ErrorMessage = "Il titolo dev'essere di al massimo {1} caratteri"),
        RegularExpression(@"^[0-9A-z\u00C0-\u00ff\s\.']+$", ErrorMessage = "Titolo non valido")]
        public string Title { get; set; }
    }
}
