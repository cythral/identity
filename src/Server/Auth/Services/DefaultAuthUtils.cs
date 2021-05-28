using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Applications;
using Brighid.Identity.Users;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Brighid.Identity.Auth
{
    public class DefaultAuthUtils : IAuthUtils
    {
        private readonly IApplicationRepository applicationRepository;
        private readonly IUserRepository userRepository;
        private readonly OpenIdConfig openIdConfig;
        private readonly OpenIddictServerOptions openIddictServerOptions;

        public DefaultAuthUtils(
            IApplicationRepository applicationRepository,
            IUserRepository userRepository,
            IOptions<OpenIdConfig> openIdConfig,
            IOptions<OpenIddictServerOptions> openIddictServerOptions
        )
        {
            this.applicationRepository = applicationRepository;
            this.userRepository = userRepository;
            this.openIdConfig = openIdConfig.Value;
            this.openIddictServerOptions = openIddictServerOptions.Value;
        }

        public async Task<ClaimsIdentity> CreateClaimsIdentityForApplication(Guid applicationId, CancellationToken cancellationToken = default)
        {
            var result = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, Claims.Name, Claims.Role);

            result.AddClaim(Claims.Name, applicationId.ToString(), Destinations.AccessToken, Destinations.IdentityToken);
            result.AddClaim(Claims.Subject, applicationId.ToString(), Destinations.AccessToken, Destinations.IdentityToken);

            var roles = await applicationRepository.FindRolesById(applicationId);
            var roleNames = roles.Select(role => $"\"{role.Name}\"");
            var roleClaim = new Claim(Claims.Role, $"[{string.Join(',', roleNames)}]", JsonClaimValueTypes.JsonArray);
            roleClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
            result.AddClaim(roleClaim);

            return result;
        }

        public async Task<ClaimsIdentity> CreateClaimsIdentityForUser(User user, CancellationToken cancellationToken = default)
        {
            var result = new ClaimsIdentity(IdentityConstants.ApplicationScheme, Claims.Name, Claims.Role);
            result.AddClaim(Claims.Name, user.Email.ToString(), Destinations.IdentityToken, Destinations.AccessToken);
            result.AddClaim(Claims.Subject, user.Id.ToString(), Destinations.IdentityToken, Destinations.AccessToken);

            var roles = await userRepository.FindRolesById(user.Id, cancellationToken);
            var roleNames = roles?.Select(role => $"\"{role.Name}\"") ?? Array.Empty<string>();
            var roleClaim = new Claim(Claims.Role, $"[{string.Join(',', roleNames)}]", JsonClaimValueTypes.JsonArray);
            roleClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
            result.AddClaim(roleClaim);

            return result;
        }

        public AuthenticationTicket CreateAuthTicket(
            ClaimsIdentity claimsIdentity,
            IEnumerable<string>? scopes = null,
            Uri? redirectUri = null,
            string authenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
        )
        {
            scopes ??= Array.Empty<string>();

            var principal = new ClaimsPrincipal(claimsIdentity);
            principal.SetResources("identity.brigh.id");
            principal.SetScopes(scopes);

            var authProps = new AuthenticationProperties() { RedirectUri = redirectUri?.ToString() };
            return new AuthenticationTicket(principal, authProps, authenticationScheme);
        }

        public string GenerateAccessToken(AuthenticationTicket authenticationTicket)
        {
            var jwt = new JwtSecurityToken(issuer: $"https://{openIdConfig.DomainName}/",
                                                audience: "identity",
                                                claims: authenticationTicket.Principal.Claims,
                                                notBefore: DateTime.UtcNow,
                                                expires: DateTime.UtcNow.Add(TimeSpan.FromHours(1)),
                                                signingCredentials: openIddictServerOptions.SigningCredentials.First());

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
