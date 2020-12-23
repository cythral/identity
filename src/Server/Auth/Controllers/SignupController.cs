using System;
using System.Threading.Tasks;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Brighid.Identity.Auth
{
    [Route("/signup")]
    public class SignupController : Controller
    {
        private const string defaultRedirectUri = "/";
        private readonly SignInManager<User> signinManager;
        private readonly UserManager<User> userManager;
        private readonly IUserService userService;

        public SignupController(
            SignInManager<User> signinManager,
            UserManager<User> userManager,
            IUserService userService
        )
        {
            this.signinManager = signinManager;
            this.userManager = userManager;
            this.userService = userService;
        }

        [HttpGet]
        public IActionResult Render([FromQuery(Name = "redirect_uri")] string? destination = defaultRedirectUri)
        {
            destination ??= defaultRedirectUri;
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

                var user = await userService.Create(request.Email, request.Password);
                var signinResult = await signinManager.PasswordSignInAsync(user, request.Password, false, false);

                if (!signinResult.Succeeded)
                {
                    throw new SignupException("Unable to sign in.");
                }

                return LocalRedirect(redirectUri);
            }
            catch (SignupException e)
            {
                if (e.Message != null)
                {
                    ModelState.AddModelError("signupError", e.Message);
                }
            }
            catch (AggregateException e)
            {
                foreach (var innerException in e.InnerExceptions)
                {
                    ModelState.AddModelError("signupError", innerException.Message);
                }
            }

            return Render(redirectUri);
        }
    }
}
