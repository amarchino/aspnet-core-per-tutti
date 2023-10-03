using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MyCourse.Models.InputModels.Lessons;
using MyCourse.Models.Options;
using MyCourse.Models.ViewModels.Lessons;

namespace MyCourse.Models.Services.Application.Lessons;
public class MemoryCacheLessonService : ICachedLessonService
{
    private readonly ILessonService lessonService;
    private readonly IMemoryCache memoryCache;
    private readonly IOptionsMonitor<CoursesOptions> coursesOptions;
    public MemoryCacheLessonService(ILessonService lessonService, IMemoryCache memoryCache, IOptionsMonitor<CoursesOptions> coursesOptions)
    {
        this.coursesOptions = coursesOptions;
        this.memoryCache = memoryCache;
        this.lessonService = lessonService;
    }

    public Task<LessonDetailViewModel> GetLessonAsync(int id)
    {
        return memoryCache.GetOrCreateAsync($"Lesson{id}", cacheEntry =>
        {
            cacheEntry.SetSize(1);
            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(coursesOptions.CurrentValue.CacheDuration));
            return lessonService.GetLessonAsync(id);
        })!;
    }

    public async Task<LessonDetailViewModel> CreateLessonAsync(LessonCreateInputModel inputModel)
    {
        LessonDetailViewModel viewModel = await lessonService.CreateLessonAsync(inputModel);
        memoryCache.Remove($"Course{viewModel.CourseId}");
        return viewModel;
    }

    public Task<LessonEditInputModel> GetLessonForEditingAsync(int id)
    {
        return lessonService.GetLessonForEditingAsync(id);
    }

    public async Task<LessonDetailViewModel> EditLessonAsync(LessonEditInputModel inputModel)
    {
        LessonDetailViewModel viewModel = await lessonService.EditLessonAsync(inputModel);
        memoryCache.Remove($"Lesson{viewModel.Id}");
        memoryCache.Remove($"Course{viewModel.CourseId}");
        return viewModel;
    }

    public async Task DeleteLessonAsync(LessonDeleteInputModel inputModel)
    {
        await lessonService.DeleteLessonAsync(inputModel);
        memoryCache.Remove($"Course{inputModel.CourseId}");
        memoryCache.Remove($"Lesson{inputModel.Id}");
    }
}
