using ImageMagick;
using Microsoft.Extensions.Options;
using MyCourse.Models.Exceptions.Infrastructure;
using MyCourse.Models.Options;

namespace MyCourse.Models.Services.Infrastructure;
public class MagickNetImagePersister : IImagePersister
{
    private readonly IWebHostEnvironment env;
    private readonly IOptionsMonitor<CoursesOptions> coursesOptions;
    private readonly SemaphoreSlim semaphore;

    public MagickNetImagePersister(IWebHostEnvironment env, IOptionsMonitor<CoursesOptions> coursesOptions)
    {
        this.coursesOptions = coursesOptions;
        this.env = env;

        ResourceLimits.Width = 4000;
        ResourceLimits.Height = 4000;
        semaphore = new SemaphoreSlim(2);
    }
    public async Task<string> SaveCourseImageAsync(int courseId, IFormFile formFile)
    {
        await semaphore.WaitAsync();
        try
        {
            string path = $"/Courses/{courseId}.jpg";
            string physicalPath = Path.Combine(env.WebRootPath, "Courses", $"{courseId}.jpg");

            using Stream inputStream = formFile.OpenReadStream();
            using var image = new MagickImage(inputStream);

            // Manipolare l'immagine
            // TODO: ottenere questi valori dalla configurazione
            int width = coursesOptions.CurrentValue.Image.MaxWidth;
            int height = coursesOptions.CurrentValue.Image.MaxHeight;
            image.Resize(new MagickGeometry(width, height) { FillArea = true });
            image.Crop(width, height, Gravity.Northwest);

            image.Quality = 60;
            image.Write(physicalPath, MagickFormat.Jpg);

            return path;
        }
        catch (Exception exc)
        {
            throw new ImagePersistenceException(exc);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
