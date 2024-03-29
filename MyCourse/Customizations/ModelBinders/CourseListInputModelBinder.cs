using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;

namespace MyCourse.Customizations.ModelBinders;

public class CourseListInputModelBinder : IModelBinder
{
    private readonly IOptionsMonitor<CoursesOptions> coursesOptions;
    public CourseListInputModelBinder(IOptionsMonitor<CoursesOptions> coursesOptions)
    {
        this.coursesOptions = coursesOptions;
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        // Recuperiamo i valori grazie ai value provider
        string? search = bindingContext.ValueProvider.GetValue("Search").FirstValue;
        int page = Convert.ToInt32(bindingContext.ValueProvider.GetValue("Page").FirstValue);
        string? orderBy = bindingContext.ValueProvider.GetValue("OrderBy").FirstValue;
        bool ascending = Convert.ToBoolean(bindingContext.ValueProvider.GetValue("Ascending").FirstValue);

        // Creiamo l'istanza del CourseListInputModel
        var inputModel = new CourseListInputModel(search, page, orderBy, ascending, coursesOptions.CurrentValue.PerPage, coursesOptions.CurrentValue.Order);

        // Impostiamo il risultato per notificare che la creazione è avvenuta con successo
        bindingContext.Result = ModelBindingResult.Success(inputModel);

        // Restituiamo un task completato
        return Task.CompletedTask;
    }
}
