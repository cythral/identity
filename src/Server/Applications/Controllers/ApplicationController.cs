using System;

using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Mvc;

namespace Brighid.Identity.Applications
{
    [Route(BasePath)]
    [Roles(new[]
    {
        nameof(BuiltInRole.ApplicationManager),
        nameof(BuiltInRole.Administrator)
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

    }
}
