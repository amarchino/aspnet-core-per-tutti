using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MyCourse.Models.Services.Infrastructure
{
    public class MagickNetImagePersister : IImagePersister
    {
        private readonly IWebHostEnvironment env;
        public MagickNetImagePersister(IWebHostEnvironment env)
        {
            this.env = env;

        }
        public Task<string> SaveCourseImageAsync(int courseId, IFormFile formFile)
        {
            string path = $"/Courses/{courseId}.jpg";
            string physicalPath = Path.Combine(env.WebRootPath, "Courses", $"{courseId}.jpg");

            using Stream inputStream = formFile.OpenReadStream();
            using var image = new MagickImage(inputStream);

            // Manipolare l'immagine
            // TODO: ottenere questi valori dalla configurazione
            int width = 300;
            int height = 300;
            image.Resize(new MagickGeometry(width, height){ FillArea = true });
            image.Crop(width, height, Gravity.Northwest);

            image.Quality = 60;
            image.Write(physicalPath, MagickFormat.Jpg);

            return Task.FromResult(path);
        }
    }
}
