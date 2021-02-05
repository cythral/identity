using System;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Mvc;

namespace Brighid.Identity.Applications
{
    [Route(BasePath)]
    [Roles(new[]
    {
        nameof(BuiltInRole.ApplicationManager),
        nameof(BuiltInRole.Administrator),
    })]
    public class ApplicationController : EntityController<Application, Guid, IApplicationRepository, IApplicationService>
    {
        public const string BasePath = "/api/applications";

        public ApplicationController(
            IApplicationService appService,
            IApplicationRepository appRepository
        ) : base(BasePath, appService, appRepository)
        {
        }

        protected override void SetSnsContextItems(Guid id, Application data)
        {
            data.Secret = null;
            base.SetSnsContextItems(id, data);
        }

        public override async Task<ActionResult<Application>> Create([FromBody] Application entity)
        {
            try
            {
                return await base.Create(entity);
            }
            catch (RoleNotFoundException e) { return BadRequest(new { e.Message }); }
        }
    }
}
