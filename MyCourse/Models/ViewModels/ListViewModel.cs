namespace MyCourse.Models.ViewModels;
public class ListViewModel<T>
{
    public List<T> Results { get; set; } = new List<T>();
    public int TotalCount { get; set; }
}
