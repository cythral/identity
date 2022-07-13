using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using static Brighid.Identity.Roles.BuiltInRole;

namespace Brighid.Identity.Roles
{
    [Route(BasePath)]

    public class RoleController : Controller
    {
        public const string BasePath = "/api/roles";
        private readonly IRoleService service;

        public RoleController(
            IRoleService service
        )
        {
            this.service = service;
        }

        [HttpGet]
        [Roles(new[] { nameof(Basic) })]
        public async Task<ActionResult<IEnumerable<Role>>> List()
        {
            var results = await service.List(HttpContext.RequestAborted);
            return Ok(results);
        }

        [HttpGet("{id:guid}")]
        [Roles(new[] { nameof(Basic) })]
        public virtual async Task<ActionResult<Role>> GetById(Guid id)
        {
            var result = await service.GetById(id, HttpContext.RequestAborted);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("{name}")]
        [Roles(new[] { nameof(Basic) })]
        public async Task<ActionResult<Role>> GetByName(string name)
        {
            var role = await service.GetByName(name, HttpContext.RequestAborted);
            return role == null ? NotFound() : Ok(role);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Role), (int)HttpStatusCode.Created)]
        [Roles(new[] { nameof(RoleManager), nameof(Administrator) })]
        public async Task<ActionResult<Role>> Create([FromBody] RoleRequest request)
        {
            try
            {
                var result = await service.Create(request, HttpContext.RequestAborted);
                var destination = new Uri($"{BasePath}/{result.Id}", UriKind.Relative);
                return Created(destination, result);
            }
            catch (Exception e) when (e is IValidationException)
            {
                return UnprocessableEntity(new { e.Message });
            }
            catch (AggregateException e)
            {
                return UnprocessableEntity(new
                {
                    Message = "Multiple validation errors occurred.",
                    ValidationErrors = from innerException in e.InnerExceptions
                                       where innerException is IValidationException
                                       select innerException.Message,
                });
            }
        }

        [HttpPut("{id:guid}")]
        [Roles(new[] { nameof(RoleManager), nameof(Administrator) })]
        public virtual async Task<ActionResult<Role>> UpdateById(Guid id, [FromBody] RoleRequest request)
        {
            try
            {
                var result = await service.UpdateById(id, request);
                return Ok(result);
            }
            catch (Exception e) when (e is IValidationException)
            {
                return UnprocessableEntity(new { e.Message });
            }
            catch (AggregateException e)
            {
                return UnprocessableEntity(new
                {
                    Message = "Multiple validation errors occurred.",
                    ValidationErrors = from innerException in e.InnerExceptions
                                       where innerException is IValidationException
                                       select innerException.Message,
                });
            }
        }

        [HttpDelete("{id:guid}")]
        [Roles(new[] { nameof(RoleManager), nameof(Administrator) })]
        public virtual async Task<ActionResult<Role>> DeleteById(Guid id)
        {
            var result = await service.DeleteById(id);
            return Ok(result);
        }
    }
}
