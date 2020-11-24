using System;
using System.Text.Json;
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
        private readonly ILogger<ApplicationController> logger;

        public ApplicationController(IApplicationService appService, ILogger<ApplicationController> logger)
        {
            this.appService = appService;
            this.logger = logger;
        }

        [HttpPost]
        [HttpHeader("x-amz-sns-message-type", "SubscriptionConfirmation")]
        public async Task<ActionResult> Subscribe([FromBody] SnsMessage<object> request)
        {
            logger.LogInformation($"Received request: {JsonSerializer.Serialize(request)}");
            await request.SubscribeUrl.GetAsync();
            return Ok();
        }

        [HttpPost]
        [HttpHeader("x-amz-sns-message-type", "Notification")]
        public async Task<ActionResult> HandleSns([FromBody] SnsMessage<CloudFormationRequest<Application>> request)
        {
            logger.LogInformation($"Received request: {JsonSerializer.Serialize(request)}");

            if (request?.Message == null)
            {
                throw new Exception("Expected message.");
            }

            try
            {
                var application = request.Message.ResourceProperties;
                var oldApplication = request.Message.OldResourceProperties;
                var oldName = oldApplication?.Name;
                var newName = application?.Name;
                var physicalResourceId = request.Message.PhysicalResourceId ?? newName;
                var requestType = request.Message.RequestType;

                if (application == null)
                {
                    throw new InvalidOperationException("Application properties must be specified.");
                }

                if (requestType == Update && oldName != newName)
                {
                    requestType = Create;
                    physicalResourceId = newName;
                }

                var client = requestType switch
                {
                    Create => await appService.Create(application),
                    Update => await appService.Update(application),
                    Delete => await appService.Delete(application),
                    _ => throw new NotSupportedException(),
                };

                await request.Message.ResponseURL.PutJsonAsync(new CloudFormationResponse(request.Message, physicalResourceId)
                {
                    Status = CloudFormationResponseStatus.SUCCESS,
                    Data = client,
                });
            }
#pragma warning disable CA1031
            catch (Exception e)
            {
                await request.Message.ResponseURL.PutJsonAsync(new CloudFormationResponse(request.Message)
                {
                    Status = CloudFormationResponseStatus.FAILED,
                    Reason = e.Message,
                });
            }
#pragma warning restore CA1031

            return Ok();
        }
    }
}
