using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.ViewModels;

namespace MyCourse.Customizations.ViewComponents;

[ViewComponent(Name = "PaginationBar")]
public class PaginationBarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(IPaginationInfo model)
    {
        return View(model);
    }
}
