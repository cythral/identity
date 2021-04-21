using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using static OpenIddict.Abstractions.OpenIddictConstants;

#pragma warning disable IDE0022

namespace Brighid.Identity.Auth
{
    public class DefaultSignInManager : SignInManager<User>
    {
        public DefaultSignInManager(
            UserManager<User> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<User> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<User>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<User> confirmation
        ) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }

        public override async Task SignInWithClaimsAsync(User user, AuthenticationProperties authenticationProperties, IEnumerable<Claim> additionalClaims)
        {
            var claims = new List<Claim>(additionalClaims) { };
            claims.Add(new Claim(Claims.Subject, user.Id.ToString()));
            claims.Add(new Claim(Claims.Name, user.Name));

            var roles = await UserManager.GetRolesAsync(user);
            Logger.LogInformation("User Roles: " + string.Join(',', roles));
            foreach (var role in roles)
            {
                claims.Add(new Claim(Claims.Role, role));
            }

            await base.SignInWithClaimsAsync(user, authenticationProperties, claims);
        }
    }
}
