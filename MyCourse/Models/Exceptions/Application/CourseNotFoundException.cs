namespace MyCourse.Models.Exceptions.Application;
public class CourseNotFoundException : Exception
{
    public CourseNotFoundException(int id) : base($"Course {id} not found")
    {
    }
}
