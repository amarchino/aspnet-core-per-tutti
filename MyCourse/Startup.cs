using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCourse.Customizations.Identity;
using MyCourse.Customizations.ModelBinders;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Application.Courses;
using MyCourse.Models.Services.Application.Lessons;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Identity;
using MyCourse.Models.Enums;
using MyCourse.Models.Authorization;
using MyCourse.Customizations.Authorization;

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
            services.AddReCaptcha(configuration.GetSection("ReCaptcha"));
            services.AddResponseCaching();
            services.AddRazorPages(options => {
                options.Conventions.AllowAnonymousToPage("/Privacy");
            });

            services.AddMvc(options => {
                CacheProfile homeProfile = new();
                configuration.Bind("ResponseCache:Home", homeProfile);
                options.CacheProfiles.Add("Home", homeProfile);

                options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
            });

            services.Configure<KestrelServerOptions>(configuration.GetSection("Kestrel"));

            var identityBuilder = services.AddDefaultIdentity<ApplicationUser>(options => {
                        options.Password.RequireDigit = true;
                        options.Password.RequiredLength = 8;
                        options.Password.RequireUppercase = true;
                        options.Password.RequireLowercase = true;
                        options.Password.RequireNonAlphanumeric = true;
                        options.Password.RequiredUniqueChars = 4;
                        options.SignIn.RequireConfirmedAccount = true;
                        options.Lockout.AllowedForNewUsers = true;
                        options.Lockout.MaxFailedAccessAttempts = 5;
                        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    })
                    .AddRoles<IdentityRole>()
                    .AddRoleManager<RoleManager<IdentityRole>>()
                    .AddPasswordValidator<CommonPasswordValidator<ApplicationUser>>()
                    .AddClaimsPrincipalFactory<CustomClaimsPrincipalFactory>();

            services.AddAuthorization(options => {
                options.AddPolicy(nameof(Policy.CourseAuthor), builder => builder.Requirements.Add(new CourseAuthorRequirement()));
                options.AddPolicy(nameof(Policy.CourseLimit), builder => builder.Requirements.Add(new CourseLimitRequirement(limit: 5)));
                options.AddPolicy(nameof(Policy.CourseSubscriber), builder => builder.Requirements.Add(new CourseSubscriberRequirement()));
            });

            var persistence = Persistence.EfCore;
            switch(persistence)
            {
                case Persistence.AdoNet:
                    identityBuilder.AddUserStore<AdoNetUserStore>()
                    .AddRoleStore<AdoNetRoleStore>();

                    services.AddTransient<ICourseService, AdoNetCourseService>();
                    services.AddTransient<ILessonService, AdoNetLessonService>();
                    services.AddTransient<IDatabaseAccessor, SqliteDatabaseAccessor>();
                    break;
                case Persistence.EfCore:
                    identityBuilder.AddEntityFrameworkStores<MyCourseDbContext>();

                    services.AddTransient<ICourseService, EfCoreCourseService>();
                    services.AddTransient<ILessonService, EfCoreLessonService>();
                    services.AddDbContextPool<MyCourseDbContext>(optionsBuilder => {
                        string connectionString = configuration["ConnectionStrings:Default"];
                        optionsBuilder.UseSqlite(connectionString, options => {
                            // options.EnableRetryOnFailure(3);
                        });
                    });
                    break;
            }
            services.AddTransient<ICachedCourseService, MemoryCacheCourseService>();
            services.AddTransient<ICachedLessonService, MemoryCacheLessonService>();
            services.AddTransient<IImagePersister, MagickNetImagePersister>();
            services.AddTransient<IEmailSender, MailKitEmailSender>();
            services.AddTransient<IEmailClient, MailKitEmailSender>();

            // services.AddTransient<IPaymentGateway, PaypalPaymentGateway>();
            services.AddTransient<IPaymentGateway, StripePaymentGateway>();

            services.AddScoped<IAuthorizationHandler, CourseAuthorRequirementHandler>();
            services.AddScoped<IAuthorizationHandler, CourseLimitRequirementHandler>();
            services.AddScoped<IAuthorizationHandler, CourseSubscriberRequirementHandler>();

            services.AddSingleton<IAuthorizationPolicyProvider, MultiAuthorizationPolicyProvider>();
            services.AddSingleton<ITransactionLogger, LocalTransactionLogger>();

            // Options
            services.Configure<ConnectionStringsOptions>(configuration.GetSection("ConnectionStrings"));
            services.Configure<CoursesOptions>(configuration.GetSection("Courses"));
            services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
            services.Configure<UsersOptions>(configuration.GetSection("Users"));
            services.Configure<PaypalOptions>(configuration.GetSection("Paypal"));
            services.Configure<StripeOptions>(configuration.GetSection("Stripe"));
            // services.Configure<MemoryCacheOptions>(configuration.GetSection("MemoryCache"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(WebApplication app)
        {
            IWebHostEnvironment env = app.Environment;

            if (env.IsEnvironment("Development"))
            {
                // Aggiunta automaticamente da .NET6
                // app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();
            // Endpoint Routing middleware
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseResponseCaching();

            // Endpoint middleware
            app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}").RequireAuthorization();
            app.MapRazorPages().RequireAuthorization();
        }
    }
}
