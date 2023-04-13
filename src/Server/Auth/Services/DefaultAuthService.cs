using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using OpenIddict.Abstractions;

using ValidateTokenRequestContext = OpenIddict.Server.OpenIddictServerEvents.ValidateTokenRequestContext;

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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public async Task<AuthenticationTicket> ImpersonateExchange(OpenIddictRequest request, CancellationToken cancellationToken = default)
        {
            if (request.GrantType != Constants.GrantTypes.Impersonate)
            {
                throw new InvalidOperationException("Expected impersonate grant type");
            }

            RequireAuthParameter(request, "user_id", out var userId);
            var user = await userManager.FindByIdAsync(userId);
            var identity = await authUtils.CreateClaimsIdentityForUser(user!, cancellationToken);
            var ticket = authUtils.CreateAuthTicket(identity, DefaultScopes, resources: request.Audiences);
            return ticket;
        }

        /// <inheritdoc />
        public async Task<AuthenticationTicket> PasswordExchange(
            string email,
            string password,
            Uri redirectUri,
            HttpContext httpContext,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = await userManager.FindByEmailAsync(email);
            if (user == null || !await userManager.CheckPasswordAsync(user, password))
            {
                throw new InvalidCredentialsException();
            }

            var identity = await authUtils.CreateClaimsIdentityForUser(user, cancellationToken);
            var ticket = authUtils.CreateAuthTicket(identity, DefaultScopes, redirectUri, IdentityConstants.ApplicationScheme);
            var issuer = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/";
            var accessTokenValue = authUtils.GenerateAccessToken(ticket, issuer);
            var accessToken = new AuthenticationToken { Name = "access_token", Value = accessTokenValue };
            var idTokenValue = authUtils.GenerateIdToken(ticket, user, issuer);
            var idToken = new AuthenticationToken { Name = "id_token", Value = idTokenValue };
            ticket.Properties.StoreTokens(new[] { accessToken, idToken });
            return ticket;
        }

        /// <inheritdoc />
        public ClaimsPrincipal ExtractPrincipalFromRequestContext(ValidateTokenRequestContext context)
        {
            var parameters = context.Options.TokenValidationParameters.Clone();
            parameters.ValidIssuer ??= context.BaseUri?.AbsoluteUri;
            parameters.ValidAudience = context.BaseUri?.AbsoluteUri;
            parameters.ValidateLifetime = true;

            var result = context.Options.JsonWebTokenHandler.ValidateToken(context.Request.AccessToken, parameters);
            return result.IsValid
                ? new ClaimsPrincipal(result.ClaimsIdentity)
                : throw new InvalidAccessTokenException();
        }

        private void RequireAuthParameter(OpenIddictRequest request, string parameter, out string parameterValue)
        {
            if (!request.HasParameter(parameter))
            {
                throw new MissingAuthParameterException(parameter, request.GrantType!);
            }

            parameterValue = request[parameter]!.Value.Value!.ToString()!;
        }
    }
}
