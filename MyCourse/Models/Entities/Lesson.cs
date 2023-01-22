namespace MyCourse.Models.Entities
{
    public partial class Lesson
    {
        public long Id { get; private set; }
        public long CourseId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Duration { get; private set; }

        public virtual Course Course { get; set; }
    }
}

