using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;

using OpenIddict.Abstractions;

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

        public async Task<AuthenticationTicket> ClientExchange(OpenIddictRequest request, CancellationToken cancellationToken = default)
        {
            if (!request.IsClientCredentialsGrantType())
            {
                throw new InvalidOperationException("Expected client credentials grant type.");
            }

            if (request.ClientId == null)
            {
                throw new InvalidOperationException("Expected client_id to not be null.");
            }

            var clientId = new Guid(request.ClientId);
            var identity = await authUtils.CreateClaimsIdentity(clientId, cancellationToken);

            var existingScopes = request.Scope?.Split(' ') ?? Array.Empty<string>();
            var scopes = new HashSet<string>(existingScopes).Union(defaultScopes);

            return authUtils.CreateAuthTicket(identity, scopes);
        }
    }
}
