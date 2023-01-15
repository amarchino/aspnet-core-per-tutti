namespace MyCourse
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = CreateWebHostBuilder(args).Build();
            app.MapGet("/", () => "Hello World!");
            app.Run();

        }

        public static WebApplicationBuilder CreateWebHostBuilder(string[] args) =>
            WebApplication.CreateBuilder(args);
    }
}

// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();

// app.MapGet("/", () => "Hello World!");

// app.Run();
