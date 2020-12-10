using System;
using System.Threading.Tasks;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using static AspNet.Security.OpenIdConnect.Primitives.OpenIdConnectConstants;

namespace Brighid.Identity.Auth
{
    [Route("/signup")]
    public class SignupController : Controller
    {
        private readonly SignInManager<User> signinManager;
        private readonly UserManager<User> userManager;

        public SignupController(
            SignInManager<User> signinManager,
            UserManager<User> userManager
        )
        {
            this.signinManager = signinManager;
            this.userManager = userManager;
        }

        [HttpGet]
        public IActionResult Render([FromQuery(Name = "redirect_uri")] string destination = "/")
        {
            return signinManager.IsSignedIn(User)
                ? LocalRedirect(destination)
                : View("~/Auth/Views/Signup.cshtml", new SignupRequest
                {
                    RedirectUri = new Uri(destination, UriKind.Relative)
                });
        }

#pragma warning disable IDE0046
        [HttpPost]
        public async Task<IActionResult> Signup([FromForm] SignupRequest request)
        {
            var redirectUri = request.RedirectUri.ToString();

            try
            {
                if (!ModelState.IsValid)
                {
                    throw new SignupException();
                }

                if (request.Password != request.ConfirmPassword)
                {
                    throw new SignupException("Passwords do not match.");
                }

                var user = new User { UserName = request.Username, Email = request.Username };
                var createUserResult = await userManager.CreateAsync(user, request.Password);
                if (!createUserResult.Succeeded)
                {
                    throw new SignupException("An unknown error occurred.");
                }

                var assignUserToRoleResult = await userManager.AddToRoleAsync(user, "Basic");
                if (!assignUserToRoleResult.Succeeded)
                {
                    throw new SignupException("Could not add user to Basic Role");
                }

                var signinResult = await signinManager.PasswordSignInAsync(user, request.Password, false, false);
                if (!signinResult.Succeeded)
                {
                    throw new SignupException("Could not sign in user.");
                }

                return LocalRedirect(redirectUri);
            }
            catch (SignupException e)
            {
                if (e.Message != null)
                {
                    ModelState.AddModelError(string.Empty, e.Message);
                }

                return Render(redirectUri);
            }
        }
    }
}
