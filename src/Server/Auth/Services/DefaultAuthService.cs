using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AspNet.Security.OpenIdConnect.Primitives;

using Microsoft.AspNetCore.Authentication;

namespace Brighid.Identity.Auth
{
    public class DefaultAuthService : IAuthService
    {
        private static readonly string[] defaultScopes = { "openid" };
        private readonly IAuthUtils authUtils;

        public DefaultAuthService(
            IAuthUtils authUtils
        )
        {
            this.authUtils = authUtils;
        }

        public async Task<AuthenticationTicket> ClientExchange(OpenIdConnectRequest request, CancellationToken cancellationToken = default)
        {
            if (!request.IsClientCredentialsGrantType())
            {
                throw new InvalidOperationException("Expected client credentials grant type.");
            }

            var clientId = new Guid(request.ClientId);
            var identity = await authUtils.CreateClaimsIdentity(clientId, cancellationToken);

            var existingScopes = request.Scope?.Split(' ') ?? Array.Empty<string>();
            var scopes = new HashSet<string>(existingScopes).Union(defaultScopes);

            return authUtils.CreateAuthTicket(identity, scopes);
        }
    }
}
