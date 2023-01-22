using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyCourse.Models.Entities;

namespace MyCourse.Models.Services.Infrastructure
{

    public partial class MyCourseDbContext : DbContext
    {

        public MyCourseDbContext(DbContextOptions<MyCourseDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Lesson> Lessons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Courses");
                entity.HasKey(course => course.Id); // Opzionale se il campo si chiama Id o CourseId
                // entity.HasKey(course => new { course.Id, course.Author }); // Per chiavi primarie composite

                entity.OwnsOne(course => course.CurrentPrice, builder => {
                    // Opzionale se il nome delle colonne coincide
                    builder
                        .Property(money => money.Currency)
                        .HasConversion<string>()
                        .HasColumnName("CurrentPrice_Currency");
                    builder.Property(money => money.Amount).HasColumnName("CurrentPrice_Amount");
                });
                entity.OwnsOne(course => course.FullPrice, builder => {
                    builder.Property(money => money.Currency).HasConversion<string>();
                });

                entity.HasMany(course => course.Lessons)
                    .WithOne(lesson => lesson.Course)
                    .HasForeignKey(lesson => lesson.CourseId); // Opzionale se si chiama CourseId
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
}

