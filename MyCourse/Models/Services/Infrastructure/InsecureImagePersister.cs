namespace MyCourse.Models.Services.Infrastructure;
public class InsecureImagePersister : IImagePersister
{
    private readonly IWebHostEnvironment env;

    public InsecureImagePersister(IWebHostEnvironment env)
    {
        this.env = env;
    }

    public async Task<string> SaveCourseImageAsync(int courseId, IFormFile formFile)
    {
        string path = $"/Courses/{courseId}.jpg";
        string physicalPath = Path.Combine(env.WebRootPath, "Courses", $"{courseId}.jpg");
        using FileStream fileStream = File.OpenWrite(physicalPath);
        await formFile.CopyToAsync(fileStream);
        return path;
    }
}
