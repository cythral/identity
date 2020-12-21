using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AspNet.Security.OpenIdConnect.Extensions;

using Brighid.Identity.Applications;

using Microsoft.AspNetCore.Authentication;

using OpenIddict.Server;

using static AspNet.Security.OpenIdConnect.Primitives.OpenIdConnectConstants;

namespace Brighid.Identity.Auth
{
    public class DefaultAuthUtils : IAuthUtils
    {
        private readonly IApplicationRoleRepository roleRepository;

        public DefaultAuthUtils(
            IApplicationRoleRepository roleRepository
        )
        {
            this.roleRepository = roleRepository;
        }

        public async Task<ClaimsIdentity> CreateClaimsIdentity(Guid applicationId, CancellationToken cancellationToken = default)
        {
            var roles = await roleRepository.FindRolesForApplication(applicationId, cancellationToken);
            var roleNames = roles.Select(role => $"\"{role.Name}\"");

            var result = new ClaimsIdentity(OpenIddictServerDefaults.AuthenticationScheme, Claims.Name, Claims.Role);

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
            var authProps = new AuthenticationProperties();
            var ticket = new AuthenticationTicket(principal, authProps, OpenIddictServerDefaults.AuthenticationScheme);

            ticket.SetResources("identity.brigh.id");
            ticket.SetScopes(scopes);

            return ticket;
        }
    }
}
