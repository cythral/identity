using System.Threading.Tasks;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Brighid.Identity.Auth
{
    [Route("/logout")]
    public class LogoutController : Controller
    {
        private readonly SignInManager<User> signInManager;

        public LogoutController(
            SignInManager<User> signInManager
        )
        {
            this.signInManager = signInManager;
        }

        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return LocalRedirect("/login?redirect_uri=%2F");
        }
    }
}
