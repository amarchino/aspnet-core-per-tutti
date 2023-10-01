using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Controllers;
using MyCourse.Customizations.ValidationAttributes;

namespace MyCourse.Models.InputModels.Courses;
public class CourseCreateInputModel
{
    [Required(ErrorMessage = "Il titolo è obbligatorio"),
    MinLength(10, ErrorMessage = "Il titolo dev'essere di almeno {1} caratteri"),
    MaxLength(100, ErrorMessage = "Il titolo dev'essere di al massimo {1} caratteri"),
    RegularExpression(@"^[\w\s\.]+$", ErrorMessage = "Titolo non valido"),
    NotNull(ErrorMessage = "Il titolo non deve essere null"),
    Remote(action: nameof(CoursesController.IsTitleAvailable), controller: "Courses", ErrorMessage = "Il titolo esiste già")]
    public string Title { get; set; }
}
