using System.Threading.Tasks;

using Brighid.Identity.Roles;
using Brighid.Identity.Users;

using Microsoft.AspNetCore.Mvc;

#pragma warning disable IDE0050

namespace Brighid.Identity.LoginProviders
{
    [Route("/api/login-providers")]
    [Roles(new[]
    {
        nameof(BuiltInRole.Basic),
        nameof(BuiltInRole.Administrator),
    })]
    public class LoginProviderController : Controller
    {
        private readonly string[] UserEmbeds = new[] { "Roles", "Logins" };

        private readonly IUserRepository repository;

        public LoginProviderController(
            IUserRepository repository
        )
        {
            this.repository = repository;
        }

        [HttpGet("{loginProvider}/{providerKey}")]
        public async Task<ActionResult<User>> GetUserByLoginProvider(string loginProvider, string providerKey)
        {
            var result = await repository.FindByLogin(loginProvider, providerKey, UserEmbeds);
            return result == null ? NotFound() : Ok(result);
        }
    }
}
