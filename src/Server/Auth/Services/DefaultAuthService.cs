using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

using OpenIddict.Abstractions;

namespace Brighid.Identity.Auth
{
    public class DefaultAuthService : IAuthService
    {
        private static readonly string[] DefaultScopes = { "openid" };
        private readonly IAuthUtils authUtils;
        private readonly UserManager<User> userManager;

        public DefaultAuthService(
            IAuthUtils authUtils,
            UserManager<User> userManager
        )
        {
            this.authUtils = authUtils;
            this.userManager = userManager;
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
            var identity = await authUtils.CreateClaimsIdentityForApplication(clientId, cancellationToken);

            var existingScopes = request.Scope?.Split(' ') ?? Array.Empty<string>();
            var scopes = new HashSet<string>(existingScopes).Union(DefaultScopes);

            return authUtils.CreateAuthTicket(identity, scopes);
        }

        public async Task<AuthenticationTicket> PasswordExchange(string email, string password, Uri redirectUri, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = await userManager.FindByEmailAsync(email);
            if (user == null || !await userManager.CheckPasswordAsync(user, password))
            {
                throw new InvalidCredentialsException();
            }

            var identity = await authUtils.CreateClaimsIdentityForUser(user, cancellationToken);
            var ticket = authUtils.CreateAuthTicket(identity, DefaultScopes, redirectUri, IdentityConstants.ApplicationScheme);
            var accessToken = authUtils.GenerateAccessToken(ticket);
            var jwtAccessToken = new AuthenticationToken { Name = "jwt", Value = accessToken };
            ticket.Properties.StoreTokens(new[] { jwtAccessToken });
            return ticket;
        }
    }
}
