using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Exceptions.Application
{
    public class CourseSubscriptionException : Exception
    {
        public CourseSubscriptionException(int id) : base($"Could not subscribe to course course {id}")
        {
        }
    }
}
