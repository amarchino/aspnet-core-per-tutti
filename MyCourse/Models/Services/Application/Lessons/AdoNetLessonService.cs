using System.Data;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.InputModels.Lessons;
using MyCourse.Models.ViewModels.Lessons;

namespace MyCourse.Models.Services.Application.Lessons;
public class AdoNetLessonService : ILessonService
{
    private readonly IDatabaseAccessor db;
    private readonly ILogger<AdoNetCourseService> logger;

    public AdoNetLessonService(IDatabaseAccessor db, ILogger<AdoNetCourseService> logger)
    {
        this.logger = logger;
        this.db = db;
    }
    public async Task<LessonDetailViewModel> GetLessonAsync(int id)
    {
        logger.LogInformation("Lesson {id} requested", id);
        FormattableString query = $"SELECT Id, CourseId, Title, Description, Duration FROM Lessons WHERE Id={id};";
        DataSet dataSet = await db.QueryAsync(query);
        var lessonTable = dataSet.Tables[0];
        if (lessonTable.Rows.Count != 1)
        {
            logger.LogWarning("Lesson {id} not found", id);
            throw new LessonNotFoundException(id);
        }
        return LessonDetailViewModel.FromDataRow(lessonTable.Rows[0]);
    }

    public async Task<LessonDetailViewModel> CreateLessonAsync(LessonCreateInputModel inputModel)
    {
        string title = inputModel.Title;
        FormattableString query = $@"INSERT INTO Lessons(CourseId, Title, Duration) VALUES ({inputModel.CourseId}, {title}, '00:00:00');
                SELECT last_insert_rowid();";
        int lessonId = await db.QueryScalarAsync<int>(query);
        LessonDetailViewModel lesson = await GetLessonAsync(lessonId);
        return lesson;
    }

    public async Task<LessonEditInputModel> GetLessonForEditingAsync(int id)
    {
        FormattableString query = $@"SELECT Id, CourseId, Title, Description, Duration, RowVersion, Order FROM Lessons WHERE Id={id}";
        DataSet dataSet = await db.QueryAsync(query);
        var lessonTable = dataSet.Tables[0];
        if (lessonTable.Rows.Count != 1)
        {
            logger.LogWarning("Lesson {id} not found", id);
            throw new LessonNotFoundException(id);
        }
        return LessonEditInputModel.FromDataRow(lessonTable.Rows[0]);
    }

    public async Task<LessonDetailViewModel> EditLessonAsync(LessonEditInputModel inputModel)
    {
        var affectedRows = await db.CommandAsync($@"UPDATE Lessons SET Title={inputModel.Title}, Description={inputModel.Description}, Duration={inputModel.Duration:HH':'mm':'ss} WHERE Id={inputModel.Id}, Order={inputModel.Order} AND CourseId={inputModel.CourseId} AND RowVersion={inputModel.RowVersion}");
        if (affectedRows == 0)
        {
            bool lessonExists = await db.QueryScalarAsync<bool>($"SELECT COUNT(*) FROM Lessons WHERE Id={inputModel.Id}");
            if (lessonExists)
            {
                throw new OptimisticConcurrencyException();
            }
            throw new LessonNotFoundException(inputModel.Id);
        }

        return await GetLessonAsync(inputModel.Id);
    }

    public async Task DeleteLessonAsync(LessonDeleteInputModel inputModel)
    {
        int affectedRows = await db.CommandAsync($"DELETE FROM Lessons WHERE Id={inputModel.Id}");
        if (affectedRows == 0)
        {
            throw new LessonNotFoundException(inputModel.Id);
        }
    }
}
