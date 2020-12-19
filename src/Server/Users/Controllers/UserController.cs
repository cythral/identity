using System;
using System.Text.Json;
using System.Threading.Tasks;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using static AspNet.Security.OpenIdConnect.Primitives.OpenIdConnectConstants;

namespace Brighid.Identity.Users
{
    [Route("/api/users")]
    public class UserController : Controller
    {
        private readonly string[] Embeds = new[] { "Roles.Role" };

        private readonly IUserRepository repository;

        public UserController(
            IUserRepository repository
        )
        {
            this.repository = repository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(Guid id)
        {
            var result = await repository.GetById(id, Embeds);
            return result == null ? NotFound() : Ok(result);
        }
    }
}
