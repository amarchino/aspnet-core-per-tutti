using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Exceptions.Infrastructure
{
    public class SendException : Exception
    {
        public SendException(): base("Constraint violated")
        {

        }
    }
}
