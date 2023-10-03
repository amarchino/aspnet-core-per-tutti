using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyCourse.Models.Entities;
using MyCourse.Models.Enums;
using MyCourse.Models.ValueTypes;

namespace MyCourse.Models.Services.Infrastructure;

public partial class MyCourseDbContext : IdentityDbContext<ApplicationUser>
{

    public MyCourseDbContext(DbContextOptions<MyCourseDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Course>? Courses { get; set; }
    public virtual DbSet<Lesson>? Lessons { get; set; }
    public virtual DbSet<Subscription>? Subscriptions { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Pre-convention model configuration goes here
        configurationBuilder.Properties<Currency>().HaveConversion<string>();
        configurationBuilder.Properties<decimal>().HaveConversion<float>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Owned<Money>();

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Courses");
            entity.HasKey(course => course.Id); // Opzionale se il campo si chiama Id o CourseId
                                                // entity.HasKey(course => new { course.Id, course.Author }); // Per chiavi primarie composite

            entity.HasMany(course => course.Lessons)
                .WithOne(lesson => lesson.Course)
                .HasForeignKey(lesson => lesson.CourseId); // Opzionale se si chiama CourseId

            entity.Property(course => course.RowVersion).IsRowVersion();
            entity.HasIndex(course => course.Title).IsUnique();
            entity.Property(course => course.Status).HasConversion<string>();

            entity.HasOne(course => course.AuthorUser)
                .WithMany(user => user.AuthoredCourses)
                .HasForeignKey(course => course.AuthorId);

            entity.HasMany(course => course.SubscribedUsers)
                .WithMany(user => user.SubscribedCourses)
                .UsingEntity<Subscription>(
                    entity =>
                    {
                        entity.ToTable("Subscriptions");
                    });

            entity.HasQueryFilter(course => course.Status != CourseStatus.Deleted);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.ToTable("Lessons");
            entity.HasKey(lesson => lesson.Id);

            // entity.HasOne(lesson => lesson.Course)
            //     .WithMany(course => course.Lessons)
            //     .HasForeignKey(lesson => lesson.CourseId);
        });
    }
}
