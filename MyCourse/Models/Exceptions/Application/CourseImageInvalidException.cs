using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Exceptions.Application
{
    public class CourseImageInvalidException : Exception
    {
        public CourseImageInvalidException(int id, Exception exc) : base($"Image for course {id} invalid", exc)
        {
        }
    }
}
