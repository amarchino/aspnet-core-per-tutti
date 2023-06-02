using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MyCourse.Models.Services.Application.Courses;
using MyCourse.Models.Services.Application.Lessons;

namespace MyCourse.Models.Authorization
{
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
            if(context.Resource is int)
            {
                courseId = (int)context.Resource;
            }
            else
            {
                int id = Convert.ToInt32(httpContextAccessor.HttpContext.Request.RouteValues["id"]);
                if(id == 0)
                {
                    context.Fail();
                    return;
                }
                switch(httpContextAccessor.HttpContext.Request.RouteValues["controller"].ToString().ToLowerInvariant())
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
            int courseId = context.Resource is int ? (int)context.Resource : Convert.ToInt32(httpContextAccessor.HttpContext.Request.RouteValues["id"]);
            if(courseId == 0)
            {
                context.Fail();
                return;
            }
            string authorId = await courseService.GetCourseAuthorIdAsync(courseId);

            if(authorId == userId)
            {
                context.Succeed(requirement);
                return;
            }
            context.Fail();
        }
    }
}
