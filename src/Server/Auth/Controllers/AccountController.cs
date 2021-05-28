using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Mvc;

namespace Brighid.Identity.Auth
{
    [Route("/account")]
    [Roles(new[]
    {
        nameof(BuiltInRole.Basic),
        nameof(BuiltInRole.Administrator),
    })]
    public class AccountController : Controller
    {
        private readonly ILinkStartUrlService linkStartUrlService;

        public AccountController(
            ILinkStartUrlService linkStartUrlService
        )
        {
            this.linkStartUrlService = linkStartUrlService;
        }

        [HttpGet("link/{provider}")]
        public async Task<IActionResult> RedirectToLinkStartUrl(string provider)
        {
            var url = await linkStartUrlService.GetLinkStartUrlForProvider(provider, HttpContext.RequestAborted);
            return url != null ? Redirect(url) : NotFound();
        }
    }
}
