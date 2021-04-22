using System;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Mvc;

#pragma warning disable IDE0050

namespace Brighid.Identity.Users
{
    [Route("/api/users")]
    [Roles(new[]
    {
        nameof(BuiltInRole.Basic),
        nameof(BuiltInRole.Administrator),
    })]
    public class UserController : Controller
    {
        private readonly string[] Embeds = new[] { "Roles", "Logins" };

        private readonly IUserRepository repository;
        private readonly IUserService service;

        public UserController(
            IUserRepository repository,
            IUserService service
        )
        {
            this.repository = repository;
            this.service = service;
        }

        private void ThrowIfModelStateIsInvalid()
        {
            if (!ModelState.IsValid)
            {
                throw new ModelStateException(ModelState);
            }
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<User>> Get(Guid userId)
        {
            var result = await repository.FindById(userId, Embeds);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("{userId}/logins")]
        [Policies(new[] { nameof(IdentityPolicy.RestrictedToSelfByUserId) })]
        public async Task<ActionResult<UserLogin>> CreateLogin(Guid userId, [FromBody] UserLogin loginInfo)
        {
            try
            {
                ModelState.Remove(nameof(UserLogin.User));
                ThrowIfModelStateIsInvalid();

                var result = await service.CreateLogin(userId, loginInfo);
                return Ok(result);
            }
            catch (UserNotFoundException e) { return NotFound(new { e.Message }); }
            catch (UserLoginAlreadyExistsException e) { return Conflict(new { e.Message }); }
            catch (ModelStateException e) { return BadRequest(new { e.Message, e.Errors }); }
        }
    }
}
