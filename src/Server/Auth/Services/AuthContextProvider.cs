using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

namespace Brighid.Identity.Auth
{
    public class AuthContextProvider : AuthenticationStateProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public AuthContextProvider(
            IHttpContextAccessor httpContextAccessor
        )
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var result = new AuthenticationState(httpContextAccessor.HttpContext!.User);
            return Task.FromResult(result);
        }
    }
}
