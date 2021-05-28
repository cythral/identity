using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Brighid.Identity.Auth
{
    [Route("/oauth2/")]
    public class OAuthController : Controller
    {
        private readonly GetOpenIdConnectRequest getOpenIdConnectRequest;
        private readonly IAuthService authService;

        public OAuthController(
            GetOpenIdConnectRequest getOpenIdConnectRequest,
            IAuthService authService
        )
        {
            this.getOpenIdConnectRequest = getOpenIdConnectRequest;
            this.authService = authService;
        }

        [HttpPost("token")]
        public async Task<IActionResult> Exchange()
        {
            var request = getOpenIdConnectRequest(this);
            var ticket = request.GrantType switch
            {
                GrantTypes.ClientCredentials => await authService.ClientExchange(request, HttpContext.RequestAborted),
                _ => throw new InvalidOperationException($"Grant Type {request.GrantType} Not Supported."),
            };

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }
    }
}
