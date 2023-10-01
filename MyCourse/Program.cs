﻿using Microsoft.AspNetCore.Builder;

namespace MyCourse
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            Startup startup = new (builder.Configuration);

            // Aggiungere i servizi per la dependency injection (metodo ConfigureServices)
            startup.ConfigureServices(builder.Services);

            WebApplication app = builder.Build();
            // Usiamo i middleware (metodo Configure)
            startup.Configure(app);

            app.Run();
        }

    }
}
