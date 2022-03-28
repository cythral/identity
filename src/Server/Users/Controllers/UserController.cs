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

        [HttpGet("{userId}")]
        public async Task<ActionResult<User>> Get(Guid userId)
        {
            var result = await repository.FindById(userId);

            if (result == null)
            {
                return NotFound();
            }

            await repository.LoadCollection(result, nameof(Users.User.Roles));
            await repository.LoadCollection(result, nameof(Users.User.Logins));
            return Ok(result);
        }

        [HttpPatch("{userId}/debug-mode", Name = "Users:SetDebugMode")]
        public async Task<ActionResult> SetDebugMode(Guid userId, [FromBody] bool enabled)
        {
            try
            {
                await service.SetDebugMode(userId, enabled, HttpContext.RequestAborted);
                return NoContent();
            }
            catch (UserNotFoundException exception)
            {
                return NotFound(new { exception.Message });
            }
        }

        [HttpPost("{userId}/logins", Name = "Users:CreateLogin")]
        [Policies(new[] { nameof(IdentityPolicy.RestrictedToSelfByUserId) })]
        public async Task<ActionResult<UserLogin>> CreateLogin(Guid userId, [FromBody] CreateUserLoginRequest loginInfo)
        {
            try
            {
                ModelState.Remove(nameof(UserLogin.User));
                ThrowIfModelStateIsInvalid();

                var result = await service.CreateLogin(userId, loginInfo);
                return Ok(result);
            }
            catch (UserNotFoundException e)
            {
                return NotFound(new { e.Message });
            }
            catch (UserLoginAlreadyExistsException e)
            {
                return Conflict(new { e.Message });
            }
            catch (ModelStateException e)
            {
#pragma warning disable IDE0037
                return BadRequest(new { e.Message, Errors = e.Errors });
            }
        }

        private void ThrowIfModelStateIsInvalid()
        {
            if (!ModelState.IsValid)
            {
                throw new ModelStateException(ModelState);
            }
        }
    }
}
