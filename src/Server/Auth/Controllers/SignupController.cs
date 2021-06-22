using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Brighid.Identity.Auth
{
    [Route("/signup")]
    public class SignupController : Controller
    {
        private const string DefaultRedirectUri = "/";
        private readonly SignInManager<User> signinManager;
        private readonly IAuthService authService;
        private readonly IUserService userService;

        public SignupController(
            SignInManager<User> signinManager,
            IAuthService authService,
            IUserService userService
        )
        {
            this.signinManager = signinManager;
            this.authService = authService;
            this.userService = userService;
        }

        [HttpGet]
        public IActionResult Render([FromQuery(Name = "redirect_uri")] string? destination = DefaultRedirectUri)
        {
            destination ??= DefaultRedirectUri;
            return signinManager.IsSignedIn(User)
                ? LocalRedirect(destination)
                : View("~/Auth/Views/Signup.cshtml", new SignupRequest
                {
                    RedirectUri = new Uri(destination, UriKind.Relative),
                });
        }

#pragma warning disable IDE0046
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup([FromForm] SignupRequest request)
        {
            var redirectUri = request.RedirectUri.ToString();
            var describer = new IdentityErrorDescriber();
            var blacklistedErrors = new HashSet<string> { describer.DuplicateUserName(request.Email).Description };

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
                var ticket = await authService.PasswordExchange(request.Email, request.Password, request.RedirectUri, HttpContext.RequestAborted);
                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }
            catch (InvalidCredentialsException)
            {
                ModelState.AddModelError("signupError", "Unable to sign in.");
            }
            catch (SignupException e)
            {
                if (e.Message != null && !blacklistedErrors.Contains(e.Message))
                {
                    ModelState.AddModelError("signupError", e.Message);
                }
            }
            catch (AggregateException e)
            {
                foreach (var innerException in e.InnerExceptions)
                {
                    if (!blacklistedErrors.Contains(innerException.Message))
                    {
                        ModelState.AddModelError("signupError", innerException.Message);
                    }
                }
            }

            return Render(redirectUri);
        }
    }
}
