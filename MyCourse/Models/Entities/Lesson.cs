namespace MyCourse.Models.Entities;
public partial class Lesson
{
    public int Id { get; private set; }
    public int CourseId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int Order { get; private set; }
    public TimeSpan Duration { get; private set; }
    public string RowVersion { get; private set; }
    public virtual Course Course { get; set; }

    public Lesson(string title, int courseId)
    {
        ChangeTitle(title);
        CourseId = courseId;
        Order = 1000;
        Duration = TimeSpan.FromSeconds(0);
    }

    public void ChangeTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            throw new ArgumentException("A lesson must have a title");
        }
        Title = newTitle;
    }

    public void changeDescription(string description)
    {
        Description = description;
    }

    public void changeDuration(TimeSpan duration)
    {
        Duration = duration;
    }

    public void ChangeOrder(int order)
    {
        Order = order;
    }
}

