using System;
using System.Text.Json;
using System.Threading.Tasks;

using Brighid.Identity.Roles;
using Brighid.Identity.Sns;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Brighid.Identity.Applications
{
    [Route("/api/applications")]
    [Roles(new[]
    {
        nameof(BuiltInRole.ApplicationManager),
        nameof(BuiltInRole.Administrator)
    })]
    public class ApplicationController : Controller
    {
        private readonly IApplicationService appService;
        private readonly IApplicationRepository appRepository;
        private readonly ILogger<ApplicationController> logger;

        public ApplicationController(
            IApplicationService appService,
            IApplicationRepository appRepository,
            ILogger<ApplicationController> logger
        )
        {
            this.appService = appService;
            this.appRepository = appRepository;
            this.logger = logger;
        }

        private void SetSnsContextItems(Guid id, Application data)
        {
            if ((IdentityRequestSource?)HttpContext.Items[Constants.RequestSource] == IdentityRequestSource.Sns)
            {
                HttpContext.Items[CloudFormationConstants.Id] = id;
                HttpContext.Items[CloudFormationConstants.Data] = data;
                data.Secret = null;
            }
        }

        [HttpPut("cloudformation-reply")]
        [AllowAnonymous]
        public async Task<ActionResult> CloudFormationReply([FromBody] object request)
        {
            logger.LogInformation(JsonSerializer.Serialize(request));
            await Task.CompletedTask;
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<Application>> Create([FromBody] Application application)
        {
            var result = await appService.Create(application);
            var destination = new Uri($"/api/applications/{application.Id}", UriKind.Relative);
            SetSnsContextItems(result.Id, result);

            return Created(destination, result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Application>> Get(Guid id)
        {
            var result = await appRepository.GetById(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Application>> Update(Guid id, [FromBody] Application application)
        {
            var result = await appService.Update(id, application);
            SetSnsContextItems(id, result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Application>> Delete(Guid id)
        {
            var result = await appService.Delete(id);
            SetSnsContextItems(id, result);

            return Ok(result);
        }
    }
}
