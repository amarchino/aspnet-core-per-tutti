using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MyCourse.Models.Authorization
{
    public class CourseLimitRequirement : IAuthorizationRequirement
    {
        public int Limit { get; }

        public CourseLimitRequirement(int limit)
        {
            Limit = limit;
        }
    }
}
