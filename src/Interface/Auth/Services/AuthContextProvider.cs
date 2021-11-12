using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

using Majorsoft.Blazor.Extensions.BrowserStorage;

using Microsoft.AspNetCore.Components.Authorization;

namespace Brighid.Identity.Interface.Auth
{
    public class AuthContextProvider : AuthenticationStateProvider
    {
        private readonly ICookieStoreService cookieStoreService;
        private readonly JwtSecurityTokenHandler securityTokenHandler;
        private ClaimsPrincipal? currentPrincipal;
        private JwtSecurityToken? currentSecurityToken;

        public AuthContextProvider(
            ICookieStoreService cookieStoreService,
            JwtSecurityTokenHandler securityTokenHandler
        )
        {
            this.cookieStoreService = cookieStoreService;
            this.securityTokenHandler = securityTokenHandler;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var principal = await GetCurrentPrincipal();
            return new AuthenticationState(principal);
        }

        private async Task<ClaimsPrincipal> GetCurrentPrincipal()
        {
            if (currentPrincipal != null && currentSecurityToken != null && currentSecurityToken.ValidTo >= DateTime.Now)
            {
                return currentPrincipal;
            }

            var identityTokenCookie = await cookieStoreService.GetAsync(".Brighid.IdentityToken");
            if (identityTokenCookie == null)
            {
                var anonymousIdentity = new ClaimsIdentity();
                return new ClaimsPrincipal(anonymousIdentity);
            }

            var identityTokenValue = identityTokenCookie.Value;
            var identityToken = securityTokenHandler.ReadJwtToken(identityTokenValue);
            if (identityToken.ValidTo <= DateTime.Now)
            {
                var anonymousIdentity = new ClaimsIdentity();
                return new ClaimsPrincipal(anonymousIdentity);
            }

            var claimsIdentity = new ClaimsIdentity(identityToken.Claims, "id_token", "name", "role");
            currentPrincipal = new ClaimsPrincipal(claimsIdentity);
            currentSecurityToken = identityToken;
            return currentPrincipal;
        }
    }
}
