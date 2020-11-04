using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

using Brighid.Identity.Models;
using Brighid.Identity.Sns;

using Flurl.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

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
            if (request.Message == null)
            {
                throw new Exception("Expected message.");
            }

            try
            {
                logger.LogInformation($"Received request: {JsonSerializer.Serialize(request)}");
                var application = request.Message.ResourceProperties;
                var oldApplication = request.Message.OldResourceProperties;
                var oldName = oldApplication.Name;
                var newName = application.Name;
                var physicalResourceId = request.Message.PhysicalResourceId ?? newName;
                var requestType = request.Message.RequestType;
                var regenerateSecret = oldApplication.Serial != application.Serial;

                if (requestType == Update && oldName != newName)
                {
                    requestType = Create;
                    physicalResourceId = newName;
                }

                var client = requestType switch
                {
                    Create => await appService.Create(application),
                    Update => await appService.Update(application, regenerateSecret),
                    Delete => await appService.Delete(application),
                    _ => throw new NotSupportedException(),
                };

                await request.Message.ResponseURL.PutJsonAsync(new CloudFormationResponse
                {
                    Status = CloudFormationResponseStatus.SUCCESS,
                    StackId = request.Message.StackId,
                    RequestId = request.Message.RequestId,
                    LogicalResourceId = request.Message.LogicalResourceId,
                    PhysicalResourceId = physicalResourceId,
                    Data = client,
                });
            }
#pragma warning disable CA1031
            catch (Exception e)
            {
                await request.Message.ResponseURL.PutJsonAsync(new CloudFormationResponse
                {
                    Status = CloudFormationResponseStatus.FAILED,
                    StackId = request.Message.StackId,
                    RequestId = request.Message.RequestId,
                    LogicalResourceId = request.Message.LogicalResourceId,
                    PhysicalResourceId = request.Message.PhysicalResourceId,
                    Reason = e.Message,
                });
            }
#pragma warning restore CA1031

            return Ok();
        }
    }
}
