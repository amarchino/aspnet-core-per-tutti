namespace MyCourse.Models.Exceptions.Application;
public class LessonNotFoundException : Exception
{
    public LessonNotFoundException(int id) : base($"Lesson {id} not found")
    {
    }
}
