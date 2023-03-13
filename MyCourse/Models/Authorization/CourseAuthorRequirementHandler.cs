using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MyCourse.Models.Services.Application.Courses;

namespace MyCourse.Models.Authorization
{
    public class CourseAuthorRequirementHandler : AuthorizationHandler<CourseAuthorRequirement>
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ICachedCourseService courseService;

        public CourseAuthorRequirementHandler(IHttpContextAccessor httpContextAccessor, ICachedCourseService courseService)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.courseService = courseService;
        }

        protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, CourseAuthorRequirement requirement)
        {
            string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int courseId = Convert.ToInt32(httpContextAccessor.HttpContext.Request.RouteValues["id"]);
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
