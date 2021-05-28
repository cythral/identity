using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Authentication;

using OpenIddict.Server.AspNetCore;

namespace Brighid.Identity.Auth
{
    [ScopedService]
    public interface IAuthUtils
    {
        Task<ClaimsIdentity> CreateClaimsIdentityForApplication(Guid applicationId, CancellationToken cancellationToken = default);

        Task<ClaimsIdentity> CreateClaimsIdentityForUser(User user, CancellationToken cancellationToken = default);

        AuthenticationTicket CreateAuthTicket(ClaimsIdentity claimsIdentity, IEnumerable<string>? scopes = null, Uri? redirectUri = null, string authenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        /// <summary>
        /// Generates an access token for an authentication ticket.
        /// </summary>
        /// <param name="authenticationTicket">The ticket to generate an access token for.</param>
        /// <returns>The resulting access token.</returns>
        string GenerateAccessToken(AuthenticationTicket authenticationTicket);
    }
}
