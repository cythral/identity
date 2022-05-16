using System;
using System.Net;
using System.Security;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Mvc;

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
        [ExceptionMapping<UserNotFoundException>(HttpStatusCode.NotFound)]
        public async Task<ActionResult<User>> Get(Guid userId)
        {
            var result = await repository.FindById(userId) ?? throw new UserNotFoundException(userId);
            await repository.LoadCollection(result, nameof(Users.User.Roles));
            await repository.LoadCollection(result, nameof(Users.User.Logins));
            return Ok(result);
        }

        [HttpPatch("{userId}/debug-mode", Name = "Users:SetDebugMode")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ExceptionMapping<UserNotFoundException>(HttpStatusCode.NotFound)]
        [ExceptionMapping<SecurityException>(HttpStatusCode.Forbidden)]
        public async Task<ActionResult> SetDebugMode(Guid userId, [FromBody] bool enabled)
        {
            await service.SetDebugMode(HttpContext.User, userId, enabled, HttpContext.RequestAborted);
            return NoContent();
        }

        [HttpPost("{userId}/logins", Name = "Users:CreateLogin")]
        [Policies(new[] { nameof(IdentityPolicy.RestrictedToSelfByUserId) })]
        [ExceptionMapping<UserNotFoundException>(HttpStatusCode.NotFound)]
        [ExceptionMapping<UserLoginAlreadyExistsException>(HttpStatusCode.Conflict)]
        [ExceptionMapping<ModelStateException>(HttpStatusCode.BadRequest)]
        public async Task<ActionResult<UserLogin>> CreateLogin(Guid userId, [FromBody] CreateUserLoginRequest loginInfo)
        {
            ModelState.Remove(nameof(UserLogin.User));
            ThrowIfModelStateIsInvalid();

            var result = await service.CreateLogin(userId, loginInfo, HttpContext.RequestAborted);
            return Ok(result);
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
