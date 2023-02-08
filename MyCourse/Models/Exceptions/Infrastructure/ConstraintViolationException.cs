using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Exceptions.Infrastructure
{
    public class ConstraintViolationException : Exception
    {
        public ConstraintViolationException(Exception innerException): base("Constraint violated", innerException)
        {

        }
    }
}
