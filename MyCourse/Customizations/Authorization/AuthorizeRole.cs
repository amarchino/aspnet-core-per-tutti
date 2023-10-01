using Microsoft.AspNetCore.Authorization;
using MyCourse.Models.Enums;

namespace MyCourse.Customizations.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class AuthorizeRoleAttribute : AuthorizeAttribute
{
    public AuthorizeRoleAttribute(params Role[] roles)
    {
        Roles = string.Join(",", roles.Select(role => role.ToString()));
    }
}
