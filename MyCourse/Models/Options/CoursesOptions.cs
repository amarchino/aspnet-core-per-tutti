namespace MyCourse.Models.Options;

public class CoursesOptions
{
    public int PerPage { get; set; }
    public int InHome { get; set; }
    public long CacheDuration { get; set; }
    public CoursesOrderOptions Order { get; set; } = new();
    public CoursesImageOptions Image { get; set; } = new();
}

public class CoursesOrderOptions
{
    public string By { get; set; } = "";
    public bool Ascending { get; set; }
    public string[] Allow { get; set; } = new string[0];
}

public class CoursesImageOptions
{
    public int MaxWidth { get; set; }
    public int MaxHeight { get; set; }
}
