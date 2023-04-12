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
        private readonly IOptionsMonitor<OpenIddictServerOptions> openIddictServerOptions;

        public DefaultAuthUtils(
            IApplicationRepository applicationRepository,
            IUserRepository userRepository,
            IOptionsMonitor<OpenIddictServerOptions> openIddictServerOptions
        )
        {
            this.applicationRepository = applicationRepository;
            this.userRepository = userRepository;
            this.openIddictServerOptions = openIddictServerOptions;
        }

        /// <inheritdoc />
        /// <todo>Handle cases where the application's roles are not found.</todo>
        public async Task<ClaimsIdentity> CreateClaimsIdentityForApplication(Guid applicationId, CancellationToken cancellationToken = default)
        {
            var result = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, Claims.Name, Claims.Role);

            var nameClaim = new Claim(Claims.Name, applicationId.ToString());
            nameClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
            result.AddClaim(nameClaim);

            var subClaim = new Claim(Claims.Subject, applicationId.ToString());
            subClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
            result.AddClaim(subClaim);

            var roles = (await applicationRepository.FindRolesById(applicationId))!;
            var roleNames = roles.Select(role => $"\"{role.Name}\"");
            var roleClaim = new Claim(Claims.Role, $"[{string.Join(',', roleNames)}]", JsonClaimValueTypes.JsonArray);
            roleClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
            result.AddClaim(roleClaim);

            return result;
        }

        public async Task<ClaimsIdentity> CreateClaimsIdentityForUser(User user, CancellationToken cancellationToken = default)
        {
            var result = new ClaimsIdentity(IdentityConstants.ApplicationScheme, Claims.Name, Claims.Role);

            var nameClaim = new Claim(Claims.Name, user.Email?.ToString() ?? string.Empty);
            nameClaim.SetDestinations(Destinations.IdentityToken, Destinations.AccessToken);
            result.AddClaim(nameClaim);

            var subjectClaim = new Claim(Claims.Subject, user.Id.ToString());
            subjectClaim.SetDestinations(Destinations.IdentityToken, Destinations.AccessToken);
            result.AddClaim(subjectClaim);

            var flagsClaim = new Claim("flg", ((long)user.Flags).ToString(), ClaimValueTypes.Integer64);
            flagsClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
            result.AddClaim(flagsClaim);

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
            string authenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            string?[]? resources = null
        )
        {
            resources ??= new[] { "identity.brigh.id" };
            scopes ??= Array.Empty<string>();

            var principal = new ClaimsPrincipal(claimsIdentity);

            var filteredResources = (from resource in resources where resource != null select resource).ToArray();
            principal.SetResources(filteredResources);
            principal.SetScopes(scopes);

            var authProps = new AuthenticationProperties() { RedirectUri = redirectUri?.ToString() };
            return new AuthenticationTicket(principal, authProps, authenticationScheme);
        }

        /// <inheritdoc />
        public string GenerateAccessToken(AuthenticationTicket authenticationTicket, string issuer)
        {
            var claims = authenticationTicket.Principal.Claims
            .Where(claim => claim.HasDestination(Destinations.AccessToken));

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: "identity",
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.Add(TimeSpan.FromHours(1)),
                signingCredentials: openIddictServerOptions.CurrentValue.SigningCredentials.First()
            );

            jwt.Header["typ"] = JsonWebTokenTypes.AccessToken;
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        /// <inheritdoc />
        public string GenerateIdToken(AuthenticationTicket authenticationTicket, User user, string issuer)
        {
            var claims = authenticationTicket.Principal.Claims
            .Where(claim => claim.HasDestination(Destinations.IdentityToken));

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.Add(TimeSpan.FromHours(1)),
                signingCredentials: openIddictServerOptions.CurrentValue.SigningCredentials.First()
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
