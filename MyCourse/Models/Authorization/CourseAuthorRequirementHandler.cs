using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MyCourse.Models.Authorization;
public class CourseAuthorRequirementHandler : AuthorizationHandler<CourseAuthorRequirement>
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ICachedCourseService courseService;
    private readonly ILessonService lessonService;

    public CourseAuthorRequirementHandler(IHttpContextAccessor httpContextAccessor, ICachedCourseService courseService, ILessonService lessonService)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.courseService = courseService;
        this.lessonService = lessonService;
    }

    protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, CourseAuthorRequirement requirement)
    {
        string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        int courseId;
        if (context.Resource is int)
        {
            courseId = (int)context.Resource;
        }
        else
        {
            int id = Convert.ToInt32(httpContextAccessor.HttpContext?.Request.RouteValues["id"]);
            if (id == 0)
            {
                context.Fail();
                return;
            }
            switch (httpContextAccessor.HttpContext?.Request.RouteValues["controller"]?.ToString()?.ToLowerInvariant())
            {
                case "lessons":
                    courseId = (await lessonService.GetLessonAsync(id)).CourseId;
                    break;
                case "courses":
                    courseId = id;
                    break;
                default:
                    context.Fail();
                    return;
            }
        }
        string authorId = await courseService.GetCourseAuthorIdAsync(courseId);

        if (authorId == userId)
        {
            context.Succeed(requirement);
            return;
        }
        context.Fail();
    }
}
