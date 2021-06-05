using System;
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
        private readonly IRoleService roleService;

        public ApplicationController(
            IApplicationMapper appMapper,
            IApplicationService appService,
            IApplicationRepository appRepository,
            IRoleService roleService
        )
            : base(BasePath, appMapper, appService, appRepository)
        {
            this.roleService = roleService;
        }

        protected override Task Validate(ApplicationRequest request)
        {
            roleService.ValidateRoleDelegations(request.Roles, HttpContext.User);
            return Task.CompletedTask;
        }

        protected override void SetSnsContextItems(Guid id, Application data)
        {
            data.Secret = null;
            base.SetSnsContextItems(id, data);
        }
    }
}
