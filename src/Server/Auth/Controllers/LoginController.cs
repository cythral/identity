using System;
using System.Threading.Tasks;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using static AspNet.Security.OpenIdConnect.Primitives.OpenIdConnectConstants;

namespace Brighid.Identity.Auth
{
    [Route("/login")]
    public class LoginController : Controller
    {

        private readonly SignInManager<User> signinManager;

        public LoginController(SignInManager<User> signinManager)
        {
            this.signinManager = signinManager;
        }

        [HttpGet]
        public IActionResult Render([FromQuery(Name = "redirect_uri")] string destination = "/")
        {
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
            if (!ModelState.IsValid)
            {
                return Render(redirectUri);
            }

            var result = await signinManager.PasswordSignInAsync(request.Username, request.Password, false, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Username and/or password were incorrect.");
                return Render(redirectUri);
            }

            return LocalRedirect(redirectUri);
        }
    }
}
