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
    public class UserController : EntityController<User, Guid, IUserRepository>
    {
        protected override string[] Embeds => new[] { "Roles", "Roles.User", "Roles.Role" };

        public UserController(
            IUserRepository repository
        ) : base(repository) { }

        [HttpPut]
        public ActionResult Notify([FromBody] object request)
        {
            Console.WriteLine(JsonSerializer.Serialize(request));
            return Ok();
        }
    }
}
