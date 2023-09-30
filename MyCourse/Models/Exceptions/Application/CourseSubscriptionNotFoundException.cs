using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Exceptions.Application
{
    public class CourseSubscriptionNotFoundException : Exception
    {
        public CourseSubscriptionNotFoundException(int id) : base($"Subscription to course {id} not found")
        {
        }
    }
}
