using System.Data;
using MyCourse.Models.Enums;
using MyCourse.Models.ValueTypes;

namespace MyCourse.Models.ViewModels.Courses;
public class CourseViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public string Author { get; set; } = "";
    public double Rating { get; set; }
    public Money FullPrice { get; set; } = new ();
    public Money CurrentPrice { get; set; } = new ();

    public static CourseViewModel FromDataRow(DataRow courseRow)
    {
        return new CourseViewModel
        {
            Id = Convert.ToInt32(courseRow["Id"]),
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
            )
        };
    }
}
