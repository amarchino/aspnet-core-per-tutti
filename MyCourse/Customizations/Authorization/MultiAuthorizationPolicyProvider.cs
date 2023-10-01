using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace MyCourse.Customizations.Authorization;

public class MultiAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public MultiAuthorizationPolicyProvider(IHttpContextAccessor httpContextAccessor, IOptions<AuthorizationOptions> options) : base(options)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);
        if (policy != null)
        {
            return policy;
        }
        var policyNames = policyName.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(name => name.Trim());
        var builder = new AuthorizationPolicyBuilder();
        builder.RequireAssertion(async (context) =>
        {
            var authService = httpContextAccessor.HttpContext?.RequestServices.GetService<IAuthorizationService>();
            if(authService == null)
            {
                return false;
            }
            foreach (var name in policyNames)
            {
                var result = await authService.AuthorizeAsync(context.User, context.Resource, policyName);
                if (result.Succeeded)
                {
                    return true;
                }
            }
            return false;
        });

        return builder.Build();
    }
}
