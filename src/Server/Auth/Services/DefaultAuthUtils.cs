using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Applications;

using Microsoft.AspNetCore.Authentication;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Brighid.Identity.Auth
{
    public class DefaultAuthUtils : IAuthUtils
    {
        private readonly IApplicationRepository applicationRepository;

        public DefaultAuthUtils(
            IApplicationRepository applicationRepository
        )
        {
            this.applicationRepository = applicationRepository;
        }

        public async Task<ClaimsIdentity> CreateClaimsIdentity(Guid applicationId, CancellationToken cancellationToken = default)
        {
            var roles = await applicationRepository.FindRolesById(applicationId);
            var roleNames = roles.Select(role => $"\"{role.Name}\"");

            var result = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, Claims.Name, Claims.Role);

            // TODO: Set Claims.Name to Application Name
            result.AddClaim(Claims.Name, applicationId.ToString(), Destinations.AccessToken, Destinations.IdentityToken);
            result.AddClaim(Claims.Subject, applicationId.ToString(), Destinations.AccessToken, Destinations.IdentityToken);

            var roleClaim = new Claim(Claims.Role, $"[{string.Join(',', roleNames)}]", JsonClaimValueTypes.JsonArray);
            roleClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
            result.AddClaim(roleClaim);

            return result;
        }

        public AuthenticationTicket CreateAuthTicket(ClaimsIdentity claimsIdentity, IEnumerable<string>? scopes = null)
        {
            scopes ??= Array.Empty<string>();

            var principal = new ClaimsPrincipal(claimsIdentity);
            principal.SetResources("identity.brigh.id");
            principal.SetScopes(scopes);

            var authProps = new AuthenticationProperties();
            var ticket = new AuthenticationTicket(principal, authProps, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            return ticket;
        }
    }
}
