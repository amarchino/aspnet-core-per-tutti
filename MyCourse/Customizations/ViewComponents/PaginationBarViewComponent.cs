using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.ViewModels;

namespace MyCourse.Customizations.ViewComponents
{
    [ViewComponent(Name = "PaginationBar")]
    public class PaginationBarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(CourseListViewModel model)
        {
            return View(model);
        }
    }
}
