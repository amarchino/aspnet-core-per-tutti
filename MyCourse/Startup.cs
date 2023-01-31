
using System;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCourse.Customizations.ModelBinders;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Application;
using MyCourse.Models.Services.Infrastructure;

namespace MyCourse
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCaching();
            services.AddMvc(options => {
                CacheProfile homeProfile = new CacheProfile();
                configuration.Bind("ResponseCache:Home", homeProfile);
                options.CacheProfiles.Add("Home", homeProfile);

                options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            #if DEBUG
            .AddRazorRuntimeCompilation()
            #endif
            ;

            var persistence = Persistence.EfCore;
            switch(persistence)
            {
                case Persistence.AdoNet:
                    services.AddTransient<ICourseService, AdoNetCourseService>();
                    services.AddTransient<IDatabaseAccessor, SqliteDatabaseAccessor>();
                    break;
                case Persistence.EfCore:
                    services.AddTransient<ICourseService, EfCoreCourseService>();
                    services.AddDbContextPool<MyCourseDbContext>(optionsBuilder => {
                        string connectionString = configuration["ConnectionStrings:Default"];
                        optionsBuilder.UseSqlite(connectionString);
                    });
                    break;
            }
            services.AddTransient<ICachedCourseService, MemoryCacheCourseService>();

            // Options
            services.Configure<ConnectionStringsOptions>(configuration.GetSection("ConnectionStrings"));
            services.Configure<CoursesOptions>(configuration.GetSection("Courses"));
            // services.Configure<MemoryCacheOptions>(configuration.GetSection("MemoryCache"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsEnvironment("Development"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();
            // Endpoint Routing middleware
            app.UseRouting();
            app.UseResponseCaching();

            // Endpoint middleware
            app.UseEndpoints(routeBuilder => {
                routeBuilder.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
