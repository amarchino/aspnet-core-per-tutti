using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.InputModels.Lessons;
using MyCourse.Models.ViewModels.Lessons;

namespace MyCourse.Models.Services.Application.Lessons
{
    public interface ILessonService
    {
        Task<LessonDetailViewModel> GetLessonAsync(int id);
        Task<LessonDetailViewModel> CreateLessonAsync(LessonCreateInputModel inputModel);
        Task<LessonEditInputModel> GetLessonForEditingAsync(int id);
        Task<LessonDetailViewModel> EditLessonAsync(LessonEditInputModel inputModel);
        Task DeleteLessonAsync(LessonDeleteInputModel id);
    }
}
