using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Models.Entities;
using MyCourse.Models.Exceptions;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.InputModels.Lessons;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels.Lessons;

namespace MyCourse.Models.Services.Application.Lessons
{
    public class EfCoreLessonService : ILessonService
    {
        private readonly MyCourseDbContext dbContext;
        private readonly ILogger<EfCoreLessonService> logger;

        public EfCoreLessonService(ILogger<EfCoreLessonService> logger, MyCourseDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task<LessonDetailViewModel> GetLessonAsync(int id)
        {
            IQueryable<LessonDetailViewModel> queryLinq = dbContext.Lessons
                .AsNoTracking()
                .Where(lesson => lesson.Id == id)
                .Select(lesson => LessonDetailViewModel.FromEntity(lesson));

            LessonDetailViewModel viewModel = await queryLinq.FirstOrDefaultAsync();

            if (viewModel == null)
            {
                logger.LogWarning("Lesson {id} not found", id);
                throw new LessonNotFoundException(id);
            }

            return viewModel;
        }

        public async Task<LessonDetailViewModel> CreateLessonAsync(LessonCreateInputModel inputModel)
        {
            var entity = new Lesson(inputModel.Title, inputModel.CourseId);
            dbContext.Lessons.Add(entity);
            await dbContext.SaveChangesAsync();

            return LessonDetailViewModel.FromEntity(entity);
        }

        public async Task<LessonEditInputModel> GetLessonForEditingAsync(int id)
        {
            IQueryable<LessonEditInputModel> queryLinq = dbContext.Lessons
                .AsNoTracking()
                .Where(lesson => lesson.Id == id)
                .Select(lesson => LessonEditInputModel.FromEntity(lesson));
            LessonEditInputModel viewModel = await queryLinq.FirstOrDefaultAsync();

            if (viewModel == null)
            {
                logger.LogWarning("Lesson {id} not found", id);
                throw new LessonNotFoundException(id);
            }

            return viewModel;
        }

        public async Task<LessonDetailViewModel> EditLessonAsync(LessonEditInputModel inputModel)
        {
            Lesson lesson = await dbContext.Lessons.FindAsync(inputModel.Id);
            if (lesson == null)
            {
                throw new LessonNotFoundException(inputModel.Id);
            }

            lesson.ChangeTitle(inputModel.Title);
            lesson.changeDescription(inputModel.Description);
            lesson.changeDuration(inputModel.Duration);
            lesson.ChangeOrder(inputModel.Order);
            dbContext.Entry(lesson).Property(lesson => lesson.RowVersion).OriginalValue = inputModel.RowVersion;

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new OptimisticConcurrencyException();
            }

            return LessonDetailViewModel.FromEntity(lesson);
        }

        public async Task DeleteLessonAsync(LessonDeleteInputModel inputModel)
        {
            Lesson lesson = await dbContext.Lessons.FindAsync(inputModel.Id);
            if (lesson == null)
            {
                throw new LessonNotFoundException(inputModel.Id);
            }
            dbContext.Remove(lesson);
            await dbContext.SaveChangesAsync();
        }
    }
}
