using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MyCourse.Models.Authorization;
public class CourseLimitRequirementHandler : AuthorizationHandler<CourseLimitRequirement>
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ICachedCourseService courseService;

    public CourseLimitRequirementHandler(IHttpContextAccessor httpContextAccessor, ICachedCourseService courseService)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.courseService = courseService;
    }

    protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, CourseLimitRequirement requirement)
    {
        string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        int courseCount = await courseService.GetCourseCountByAuthorIdAsync(userId);
        if (courseCount <= requirement.Limit)
        {
            context.Succeed(requirement);
            return;
        }
        context.Fail();
    }
}
