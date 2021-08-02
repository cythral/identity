using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Mvc;

#pragma warning disable IDE0050

namespace Brighid.Identity.Applications
{
    [Route(BasePath)]
    [Roles(new[]
    {
        nameof(BuiltInRole.ApplicationManager),
        nameof(BuiltInRole.Administrator),
    })]
    public class ApplicationController : ControllerBase
    {
        public const string BasePath = "/api/applications";
        private readonly IRoleService roleService;

        private readonly IApplicationService service;
        private readonly IApplicationRepository repository;
        private readonly IApplicationMapper mapper;

        public ApplicationController(
            IApplicationMapper mapper,
            IApplicationService service,
            IApplicationRepository repository,
            IRoleService roleService
        )
        {
            this.mapper = mapper;
            this.service = service;
            this.repository = repository;
            this.roleService = roleService;
        }

        [HttpGet("{id:guid}", Name = "Applications:GetById")]
        public virtual async Task<ActionResult<Application>> GetById(Guid id)
        {
            var result = await repository.FindById(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost(Name = "Applications:Create")]
        [ProducesResponseType(typeof(Application), (int)HttpStatusCode.Created)]
        public virtual async Task<ActionResult<Application>> Create([FromBody] ApplicationRequest request)
        {
            Validate(request);
            var entity = await mapper.MapRequestToEntity(request, HttpContext.RequestAborted);
            var result = await service.Create(entity);
            var primaryKey = service.GetPrimaryKey(entity);
            var destination = new Uri($"{BasePath}/{primaryKey}", UriKind.Relative);
            return Created(destination, result);
        }

        [HttpPut("{id:guid}", Name = "Applications:UpdateById")]
        public virtual async Task<ActionResult<Application>> UpdateById(Guid id, [FromBody] ApplicationRequest request)
        {
            Validate(request);
            var entity = await mapper.MapRequestToEntity(request, HttpContext.RequestAborted);
            var result = await service.UpdateById(id, entity);
            return Ok(result);
        }

        [HttpDelete("{id:guid}", Name = "Applications:DeleteById")]
        public virtual async Task<ActionResult<Application>> DeleteById(Guid id)
        {
            var result = await service.DeleteById(id);
            return Ok(result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Validate(ApplicationRequest request)
        {
            roleService.ValidateRoleDelegations(request.Roles, HttpContext.User);
        }
    }
}
