using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Brighid.Identity.Roles
{
    [Route(BasePath)]
    [Roles(new[]
    {
        nameof(BuiltInRole.RoleManager),
        nameof(BuiltInRole.Administrator),
    })]
    public class RoleController : EntityController<Role, Role, Guid, IRoleRepository, IRoleMapper, IRoleService>
    {
        public const string BasePath = "/api/roles";

        public RoleController(
            IRoleMapper mapper,
            IRoleService service,
            IRoleRepository repository
        )
            : base(BasePath, mapper, service, repository)
        {
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> List()
        {
            var results = await Repository.List();
            return Ok(results);
        }

        [HttpGet("{id:guid}")]
        public override async Task<ActionResult<Role>> GetById(Guid id)
        {
            return await base.GetById(id);
        }

        [HttpGet("{name}")]
        public async Task<ActionResult<Role>> GetByName(string name)
        {
            var role = await Repository.FindByName(name);
            return role == null ? NotFound() : Ok(role);
        }
    }
}
