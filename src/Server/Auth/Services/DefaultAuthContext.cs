using AspNet.Security.OpenIdConnect.Primitives;

using Microsoft.AspNetCore.Http;

namespace Brighid.Identity.Auth
{
    public class DefaultAuthContext : IAuthContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public DefaultAuthContext(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string UserName => httpContextAccessor.HttpContext?.User
            .FindFirst(OpenIdConnectConstants.Claims.Name)?.Value ?? string.Empty;
    }
}
