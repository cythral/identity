using Microsoft.AspNetCore.Http;

using static OpenIddict.Abstractions.OpenIddictConstants;

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
            .FindFirst(Claims.Name)?.Value ?? string.Empty;
    }
}
