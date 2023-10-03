using System.Data;
using MyCourse.Models.Entities;
using MyCourse.Models.Enums;
using MyCourse.Models.ValueTypes;
using MyCourse.Models.ViewModels.Lessons;

namespace MyCourse.Models.ViewModels.Courses;
public class CourseDetailViewModel : CourseViewModel
{
    public CourseDetailViewModel()
    {
        Lessons = new List<LessonViewModel>();
    }

    public string Description { get; set; } = "";
    public List<LessonViewModel> Lessons { get; set; } = new List<LessonViewModel>();

    public TimeSpan TotalCourseDuration
    {
        get => TimeSpan.FromSeconds(Lessons?.Sum(l => l.Duration.TotalSeconds) ?? 0);
    }

    public static new CourseDetailViewModel FromDataRow(DataRow courseRow)
    {
        return new CourseDetailViewModel
        {
            Id = Convert.ToInt32(courseRow["Id"]),
            Description = Convert.ToString(courseRow["Description"])!,
            Title = Convert.ToString(courseRow["Title"])!,
            ImagePath = Convert.ToString(courseRow["ImagePath"])!,
            Author = Convert.ToString(courseRow["Author"])!,
            Rating = Convert.ToDouble(courseRow["Rating"]),
            FullPrice = new Money(
                Enum.Parse<Currency>(Convert.ToString(courseRow["FullPrice_Currency"])!),
                Convert.ToDecimal(courseRow["FullPrice_Amount"])
            ),
            CurrentPrice = new Money(
                Enum.Parse<Currency>(Convert.ToString(courseRow["CurrentPrice_Currency"])!),
                Convert.ToDecimal(courseRow["CurrentPrice_Amount"])
            ),
            Lessons = new List<LessonViewModel>()
        };
    }

    internal static CourseDetailViewModel FromEntity(Course course)
    {
        return new CourseDetailViewModel
        {
            Id = course.Id,
            Description = course.Description!,
            Title = course.Title,
            ImagePath = course.ImagePath,
            Author = course.Author,
            Rating = course.Rating,
            FullPrice = course.FullPrice!,
            CurrentPrice = course.CurrentPrice!,
            Lessons = new List<LessonViewModel>()
        };
    }
}
