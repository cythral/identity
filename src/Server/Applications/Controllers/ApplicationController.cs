using System;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;

using Brighid.Identity.Sns;

using Flurl.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using static Brighid.Identity.Sns.CloudFormationRequestType;

namespace Brighid.Identity.Applications
{
    [Route("/api/applications")]
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

        [HttpPost]
        [HttpHeader("x-amz-sns-message-type", "SubscriptionConfirmation")]
        public async Task<ActionResult> Subscribe([FromBody] SnsMessage<object> request)
        {
            await request.SubscribeUrl.GetAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<Application>> Create([FromBody] Application application)
        {
            var result = await appService.Create(application);
            var destination = new Uri($"/api/applications/{application.Id}", UriKind.Relative);

            HttpContext.Items["identity:id"] = result.Id;
            HttpContext.Items["identity:model"] = result;

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

            HttpContext.Items["identity:id"] = id;
            HttpContext.Items["identity:model"] = result;

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Application>> Delete(Guid id)
        {
            var result = await appService.Delete(id);

            HttpContext.Items["identity:id"] = id;
            HttpContext.Items["identity:model"] = result;

            return Ok(result);
        }
    }
}
