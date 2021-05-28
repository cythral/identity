using System;
using System.Threading.Tasks;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable IDE0046 // if statements can be simplified to ternaries

namespace Brighid.Identity.Auth
{
    [Route("/login")]
    public class LoginController : Controller
    {
        private const string DefaultRedirectUri = "/";

        private readonly SignInManager<User> signinManager;
        private readonly IAuthService authService;

        public LoginController(SignInManager<User> signinManager, IAuthService authService)
        {
            this.signinManager = signinManager;
            this.authService = authService;
        }

        [HttpGet]
        public IActionResult Render([FromQuery(Name = "redirect_uri")] string? destination = DefaultRedirectUri)
        {
            destination ??= DefaultRedirectUri;

            return signinManager.IsSignedIn(User)
                ? LocalRedirect(destination)
                : View("~/Auth/Views/Login.cshtml", new LoginRequest
                {
                    RedirectUri = new Uri(destination, UriKind.Relative),
                });
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            var redirectUri = request.RedirectUri.ToString();

            try
            {
                if (!ModelState.IsValid)
                {
                    throw new LoginException();
                }

                var ticket = await authService.PasswordExchange(request.Email, request.Password, request.RedirectUri, HttpContext.RequestAborted);
                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }
            catch (LoginException e)
            {
                if (e.Message != null)
                {
                    ModelState.AddModelError("loginErrors", e.Message);
                }
            }

            return Render(redirectUri);
        }
    }
}
