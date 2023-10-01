using Microsoft.AspNetCore.Authorization;

namespace MyCourse.Models.Authorization;
public class CourseLimitRequirement : IAuthorizationRequirement
{
    public int Limit { get; }

    public CourseLimitRequirement(int limit)
    {
        Limit = limit;
    }
}
