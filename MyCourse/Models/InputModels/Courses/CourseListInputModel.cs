using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Customizations.ModelBinders;
using MyCourse.Models.Options;

namespace MyCourse.Models.InputModels.Courses
{
    [ModelBinder(BinderType = typeof(CourseListInputModelBinder))]
    public class CourseListInputModel
    {
        public string Search { get; }
        public int Page { get; }
        public string OrderBy { get; }
        public bool Ascending { get; }
        public int Limit { get; }
        public int Offset { get; }

        public CourseListInputModel(string search, int page, string orderby, bool ascending, int limit, CoursesOrderOptions orderOptions)
        {
            if (!orderOptions.Allow.Contains(orderby))
            {
                orderby = orderOptions.By;
                ascending = orderOptions.Ascending;
            }
            Search = search ?? "";
            Page = Math.Max(1, page);
            OrderBy = orderby;
            Ascending = ascending;
            Limit = Math.Max(1, limit);
            Offset = (page - 1) * limit;
        }

    }
}
