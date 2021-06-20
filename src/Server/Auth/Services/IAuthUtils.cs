using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Authentication;

using OpenIddict.Server.AspNetCore;

namespace Brighid.Identity.Auth
{
    public interface IAuthUtils
    {
        Task<ClaimsIdentity> CreateClaimsIdentityForApplication(Guid applicationId, CancellationToken cancellationToken = default);

        Task<ClaimsIdentity> CreateClaimsIdentityForUser(User user, CancellationToken cancellationToken = default);

        AuthenticationTicket CreateAuthTicket(ClaimsIdentity claimsIdentity, IEnumerable<string>? scopes = null, Uri? redirectUri = null, string authenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, string[]? resources = null);

        /// <summary>
        /// Generates an access token for an authentication ticket.
        /// </summary>
        /// <param name="authenticationTicket">The ticket to generate an access token for.</param>
        /// <returns>The resulting access token.</returns>
        string GenerateAccessToken(AuthenticationTicket authenticationTicket);

        /// <summary>
        /// Generates an id token for an authentication ticket.
        /// </summary>
        /// <param name="authenticationTicket">The ticket to generate an id token for.</param>
        /// <param name="user">The user to generate an ID token for.</param>
        /// <returns>The resulting id token.</returns>
        string GenerateIdToken(AuthenticationTicket authenticationTicket, User user);
    }
}
