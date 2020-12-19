using System;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

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

        [HttpPut]
        public async Task<ActionResult> Notify([FromBody] object request)
        {
            await Task.CompletedTask;
            Console.WriteLine(JsonSerializer.Serialize(request));
            return Ok();
        }
    }
}
