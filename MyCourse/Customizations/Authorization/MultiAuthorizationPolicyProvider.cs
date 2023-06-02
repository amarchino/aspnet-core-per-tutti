using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MyCourse.Customizations.Authorization
{
    public class MultiAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public MultiAuthorizationPolicyProvider(IHttpContextAccessor httpContextAccessor, IOptions<AuthorizationOptions> options) : base(options)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            var policy = await base.GetPolicyAsync(policyName);
            if(policy != null)
            {
                return policy;
            }
            var policyNames = policyName.Split(',', System.StringSplitOptions.RemoveEmptyEntries).Select(name => name.Trim());
            var builder = new AuthorizationPolicyBuilder();
            builder.RequireAssertion(async (context) => {
                var authService = httpContextAccessor.HttpContext.RequestServices.GetService<IAuthorizationService>();
                foreach(var name in policyNames)
                {
                    var result = await authService.AuthorizeAsync(context.User, context.Resource, policyName);
                    if(result.Succeeded)
                    {
                        return true;
                    }
                }
                return false;
            });

            return builder.Build();
        }
    }
}
