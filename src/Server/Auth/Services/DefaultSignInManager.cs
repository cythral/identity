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
        private readonly IUserRepository userRepository;

        public DefaultSignInManager(
            IUserRepository userRepository,
            UserManager<User> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<User> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<User>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<User> confirmation
        ) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            this.userRepository = userRepository;
        }

        public override async Task SignInWithClaimsAsync(User user, AuthenticationProperties authenticationProperties, IEnumerable<Claim> additionalClaims)
        {
            var claims = new List<Claim>(additionalClaims) { };
            claims.Add(new Claim(Claims.Subject, user.Id.ToString()));
            claims.Add(new Claim(Claims.Name, user.Name));

            await userRepository.LoadCollection(user, "Roles");
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(Claims.Role, role.Name));
            }

            await base.SignInWithClaimsAsync(user, authenticationProperties, claims);
        }
    }
}
