using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MyCourse.Models.Options
{
    public class CoursesOptions
    {
        public int PerPage { get; set; }
        public long CacheDuration { get; set; }
        public CoursesOrderOptions Order { get; set; }
    }

    public class CoursesOrderOptions
    {
        public string By { get; set; }
        public bool Ascending { get; set; }
        public string[] Allow { get; set; }
    }
}
