using System.Linq;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Brighid.Identity
{
    public class RestrictedToSelfPolicyHandler : AuthorizationHandler<RestrictedToSelfPolicyRequirement>
    {
#pragma warning disable IDE0083, IDE0078
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RestrictedToSelfPolicyRequirement requirement)
        {
            if (!(context.Resource is HttpContext httpContext) || context.User.IsInRole(nameof(BuiltInRole.Administrator)))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var claimValueQuery = from claim in context.User.Claims
                                  where claim.Type == requirement.ClaimType
                                  select claim.Value;

            var claimValue = claimValueQuery.FirstOrDefault();
            httpContext.Request.RouteValues.TryGetValue(requirement.RouteParameter, out var parameterValue);

            parameterValue = requirement.Parse(parameterValue as string);
            if (parameterValue == null || parameterValue.ToString() != claimValue)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
