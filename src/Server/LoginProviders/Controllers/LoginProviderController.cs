using System.Net;
using System.Security;
using System.Threading.Tasks;

using Brighid.Identity.Roles;
using Brighid.Identity.Users;

using Microsoft.AspNetCore.Mvc;

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
        private readonly IUserService service;

        public LoginProviderController(
            IUserService service
        )
        {
            this.service = service;
        }

        [HttpGet("{loginProvider}/{providerKey}", Name = "LoginProviders:GetUserByLoginProviderKey")]
        [ExceptionMapping<UserLoginNotFoundException>(HttpStatusCode.NotFound)]
        public async Task<ActionResult<User>> GetUserByLoginProviderKey(string loginProvider, string providerKey)
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();
            var result = await service.GetByLoginProviderKey(loginProvider, providerKey, HttpContext.RequestAborted);
            return Ok(result);
        }

        [HttpPut("{loginProvider}/{providerKey}/enabled", Name = "LoginProviders:SetLoginStatus")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ExceptionMapping<SecurityException>(HttpStatusCode.Forbidden)]
        [ExceptionMapping<UserLoginNotFoundException>(HttpStatusCode.NotFound)]
        [ExceptionMapping<InvalidPrincipalException>(HttpStatusCode.BadRequest)]

        public async Task<ActionResult> SetLoginStatus(string loginProvider, string providerKey, [FromBody] bool enabled)
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();
            await service.SetLoginStatus(HttpContext.User, loginProvider, providerKey, enabled, HttpContext.RequestAborted);
            return NoContent();
        }

        [HttpDelete("{loginProvider}/{providerKey}", Name = "LoginProviders:DeleteLogin")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ExceptionMapping<SecurityException>(HttpStatusCode.Forbidden)]
        [ExceptionMapping<UserLoginNotFoundException>(HttpStatusCode.NotFound)]
        [ExceptionMapping<InvalidPrincipalException>(HttpStatusCode.BadRequest)]
        public async Task<ActionResult> DeleteLogin(string loginProvider, string providerKey)
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();
            await service.DeleteLogin(HttpContext.User, loginProvider, providerKey, HttpContext.RequestAborted);
            return NoContent();
        }
    }
}
