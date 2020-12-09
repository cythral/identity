using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using static AspNet.Security.OpenIdConnect.Primitives.OpenIdConnectConstants;

namespace Brighid.Identity.Auth
{
    [Route("/login")]
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Render()
        {
            return View("~/Auth/Views/Login.cshtml");
        }
    }
}
