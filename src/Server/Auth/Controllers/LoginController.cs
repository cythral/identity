using System;
using System.Threading.Tasks;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using static AspNet.Security.OpenIdConnect.Primitives.OpenIdConnectConstants;

#pragma warning disable IDE0046 // if statements can be simplified to ternaries

namespace Brighid.Identity.Auth
{
    [Route("/login")]
    public class LoginController : Controller
    {
        private const string defaultRedirectUri = "/";

        private readonly SignInManager<User> signinManager;

        public LoginController(SignInManager<User> signinManager)
        {
            this.signinManager = signinManager;
        }

        [HttpGet]
        public IActionResult Render([FromQuery(Name = "redirect_uri")] string? destination = defaultRedirectUri)
        {
            destination ??= defaultRedirectUri;

            return signinManager.IsSignedIn(User)
                ? LocalRedirect(destination)
                : View("~/Auth/Views/Login.cshtml", new LoginRequest
                {
                    RedirectUri = new Uri(destination, UriKind.Relative)
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

                var result = await signinManager.PasswordSignInAsync(request.Username, request.Password, false, false);

                if (!result.Succeeded)
                {
                    throw new LoginException("Username and/or password were incorrect.");
                }

                return LocalRedirect(redirectUri);
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
