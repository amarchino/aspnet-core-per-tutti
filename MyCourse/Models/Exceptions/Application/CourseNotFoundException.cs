using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Exceptions.Application
{
    public class CourseNotFoundException : Exception
    {
        public CourseNotFoundException(int id) : base($"Course {id} not found")
        {
        }
    }
}
