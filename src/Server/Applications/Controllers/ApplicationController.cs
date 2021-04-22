using System;
using System.Linq;
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
    public class ApplicationController : EntityController<Application, ApplicationRequest, Guid, IApplicationRepository, IApplicationMapper, IApplicationService>
    {
        public const string BasePath = "/api/applications";

        public ApplicationController(
            IApplicationMapper appMapper,
            IApplicationService appService,
            IApplicationRepository appRepository
        ) : base(BasePath, appMapper, appService, appRepository)
        {
        }

        protected override void SetSnsContextItems(Guid id, Application data)
        {
            data.Secret = null;
            base.SetSnsContextItems(id, data);
        }

        public override async Task<ActionResult<Application>> Create([FromBody] ApplicationRequest request)
        {
            try
            {
                return await base.Create(request);
            }
            catch (RoleNotFoundException e) { return BadRequest(new { e.Message }); }
            catch (AggregateException e)
            {
                return BadRequest(new
                {
                    Message = "Multiple validation errors occurred.",
                    ValidationErrors = from innerException in e.InnerExceptions select innerException.Message,
                });
            }
        }
    }
}
