using System;
using System.Net.Http;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Sns;

using FluentAssertions;

using Flurl.Http.Testing;

using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

using static NSubstitute.Arg;

namespace Brighid.Identity.Applications
{
    public class ApplicationControllerTests
    {
        [Test, Auto]
        public async Task HandleSns_CallsCreate(
            string stackId,
            string requestId,
            string logicalResourceId,
            Uri responseUrl,
            OpenIddictApplication clientApp,
            Application application,
            [Frozen] IApplicationService appService,
            [Target] ApplicationController appController
        )
        {
            using var httpContext = new HttpTest();
            httpContext.RespondWith("OK", 200);
            appService.Create(Any<Application>()).Returns(clientApp);

            await appController.HandleSns(new SnsMessage<CloudFormationRequest<Application>>
            {
                Message = new CloudFormationRequest<Application>
                {
                    ResponseURL = responseUrl,
                    StackId = stackId,
                    RequestId = requestId,
                    LogicalResourceId = logicalResourceId,
                    RequestType = CloudFormationRequestType.Create,
                    ResourceProperties = application,
                }
            });

            await appService.Received().Create(Is(application));
            await appService.DidNotReceive().Update(Any<Application>());
            await appService.DidNotReceive().Delete(Any<Application>());

            httpContext
            .ShouldHaveCalled(responseUrl.ToString())
            .WithVerb(HttpMethod.Put)
            .WithRequestJson(new CloudFormationResponse
            {
                Status = CloudFormationResponseStatus.SUCCESS,
                StackId = stackId,
                RequestId = requestId,
                LogicalResourceId = logicalResourceId,
                PhysicalResourceId = application.Name,
                Data = clientApp,
            });
        }

        [Test, Auto]
        public async Task HandleSns_CallsUpdate_AndRegenerateClientSecret_IfSerialIsDifferent(
            string stackId,
            string requestId,
            string logicalResourceId,
            string physicalResourceId,
            string name,
            Uri responseUrl,
            OpenIddictApplication clientApp,
            [Frozen] IApplicationService appService,
            [Target] ApplicationController appController
        )
        {
            using var httpContext = new HttpTest();
            httpContext.RespondWith("OK", 200);
            appService.Update(Any<Application>(), Any<bool>()).Returns(clientApp);

            var oldApplication = new Application { Name = name, Serial = 1 };
            var newApplication = new Application { Name = name, Serial = 2 };

            await appController.HandleSns(new SnsMessage<CloudFormationRequest<Application>>
            {
                Message = new CloudFormationRequest<Application>
                {
                    ResponseURL = responseUrl,
                    StackId = stackId,
                    RequestId = requestId,
                    LogicalResourceId = logicalResourceId,
                    PhysicalResourceId = physicalResourceId,
                    RequestType = CloudFormationRequestType.Update,
                    ResourceProperties = newApplication,
                    OldResourceProperties = oldApplication,
                }
            });

            await appService.Received().Update(Is(newApplication), Is(true));
            await appService.DidNotReceive().Create(Any<Application>());
            await appService.DidNotReceive().Delete(Any<Application>());

            httpContext
            .ShouldHaveCalled(responseUrl.ToString())
            .WithVerb(HttpMethod.Put)
            .WithRequestJson(new CloudFormationResponse
            {
                Status = CloudFormationResponseStatus.SUCCESS,
                StackId = stackId,
                RequestId = requestId,
                LogicalResourceId = logicalResourceId,
                PhysicalResourceId = physicalResourceId,
                Data = clientApp,
            });
        }

        [Test, Auto]
        public async Task HandleSns_CallsUpdate_WithoutRegenerateSecret_IfSerialIsSame(
            string stackId,
            string requestId,
            string logicalResourceId,
            string physicalResourceId,
            string name,
            Uri responseUrl,
            OpenIddictApplication clientApp,
            [Frozen] IApplicationService appService,
            [Target] ApplicationController appController
        )
        {
            using var httpContext = new HttpTest();
            httpContext.RespondWith("OK", 200);
            appService.Update(Any<Application>(), Any<bool>()).Returns(clientApp);

            var oldApplication = new Application { Name = name, Serial = 1 };
            var newApplication = new Application { Name = name, Serial = 1 };

            await appController.HandleSns(new SnsMessage<CloudFormationRequest<Application>>
            {
                Message = new CloudFormationRequest<Application>
                {
                    ResponseURL = responseUrl,
                    StackId = stackId,
                    RequestId = requestId,
                    LogicalResourceId = logicalResourceId,
                    PhysicalResourceId = physicalResourceId,
                    RequestType = CloudFormationRequestType.Update,
                    ResourceProperties = newApplication,
                    OldResourceProperties = oldApplication,
                }
            });

            await appService.Received().Update(Is(newApplication), Is(false));
            await appService.DidNotReceive().Create(Any<Application>());
            await appService.DidNotReceive().Delete(Any<Application>());

            httpContext
            .ShouldHaveCalled(responseUrl.ToString())
            .WithVerb(HttpMethod.Put)
            .WithRequestJson(new CloudFormationResponse
            {
                Status = CloudFormationResponseStatus.SUCCESS,
                StackId = stackId,
                RequestId = requestId,
                LogicalResourceId = logicalResourceId,
                PhysicalResourceId = physicalResourceId,
                Data = clientApp,
            });
        }

        [Test, Auto]
        public async Task HandleSns_CallsCreate_IfApplicationNameIsDifferent_AndRespondsWithNewNameAsPhysicalResourceId(
            string stackId,
            string requestId,
            string logicalResourceId,
            string physicalResourceId,
            string oldName,
            string newName,
            Uri responseUrl,
            OpenIddictApplication clientApp,
            [Frozen] IApplicationService appService,
            [Target] ApplicationController appController
        )
        {
            using var httpContext = new HttpTest();
            httpContext.RespondWith("OK", 200);
            appService.Create(Any<Application>()).Returns(clientApp);

            var oldApplication = new Application { Name = oldName };
            var newApplication = new Application { Name = newName };

            await appController.HandleSns(new SnsMessage<CloudFormationRequest<Application>>
            {
                Message = new CloudFormationRequest<Application>
                {
                    ResponseURL = responseUrl,
                    StackId = stackId,
                    RequestId = requestId,
                    LogicalResourceId = logicalResourceId,
                    PhysicalResourceId = physicalResourceId,
                    RequestType = CloudFormationRequestType.Update,
                    ResourceProperties = newApplication,
                    OldResourceProperties = oldApplication,
                }
            });

            await appService.Received().Create(Is(newApplication));
            await appService.DidNotReceive().Update(Any<Application>());
            await appService.DidNotReceive().Delete(Any<Application>());

            httpContext
            .ShouldHaveCalled(responseUrl.ToString())
            .WithVerb(HttpMethod.Put)
            .WithRequestJson(new CloudFormationResponse
            {
                Status = CloudFormationResponseStatus.SUCCESS,
                StackId = stackId,
                RequestId = requestId,
                LogicalResourceId = logicalResourceId,
                PhysicalResourceId = newName,
                Data = clientApp,
            });
        }

        [Test, Auto]
        public async Task HandleSns_CallsDelete_AndResponds(
            string stackId,
            string requestId,
            string logicalResourceId,
            Uri responseUrl,
            OpenIddictApplication clientApp,
            Application application,
            [Frozen] IApplicationService appService,
            [Target] ApplicationController appController
        )
        {
            using var httpContext = new HttpTest();
            httpContext.RespondWith("OK", 200);
            appService.Delete(Any<Application>()).Returns(clientApp);

            await appController.HandleSns(new SnsMessage<CloudFormationRequest<Application>>
            {
                Message = new CloudFormationRequest<Application>
                {
                    StackId = stackId,
                    RequestId = requestId,
                    LogicalResourceId = logicalResourceId,
                    ResponseURL = responseUrl,
                    RequestType = CloudFormationRequestType.Delete,
                    ResourceProperties = application,
                }
            });

            await appService.Received().Delete(Is(application));
            await appService.DidNotReceive().Create(Any<Application>());
            await appService.DidNotReceive().Update(Any<Application>());

            httpContext
            .ShouldHaveCalled(responseUrl.ToString())
            .WithVerb(HttpMethod.Put)
            .WithRequestJson(new CloudFormationResponse
            {
                Status = CloudFormationResponseStatus.SUCCESS,
                StackId = stackId,
                RequestId = requestId,
                LogicalResourceId = logicalResourceId,
                PhysicalResourceId = application.Name,
                Data = clientApp,
            });
        }

        [Test, Auto]
        public async Task HandleSns_ReturnsOk(
            Uri responseUrl,
            [Target] ApplicationController appController
        )
        {
            using var httpContext = new HttpTest();
            httpContext.RespondWith("OK", 200);

            var result = await appController.HandleSns(new SnsMessage<CloudFormationRequest<Application>>
            {
                Message = new CloudFormationRequest<Application>
                {
                    ResponseURL = responseUrl,
                    RequestType = CloudFormationRequestType.Create,
                }
            });

            result.Should().BeOfType<OkResult>();
        }

        [Test, Auto]
        public async Task HandleSns_CatchesExceptionsAndResponds(
            string stackId,
            string requestId,
            string logicalResourceId,
            string physicalResourceId,
            string errorMessage,
            Uri responseUrl,
            [Frozen] IApplicationService appService,
            [Target] ApplicationController appController
        )
        {
            using var httpContext = new HttpTest();
            httpContext.RespondWith("OK", 200);

            appService.Create(Any<Application>()).Returns<OpenIddictApplication>(x => throw new Exception(errorMessage));

            Func<Task> func = async () => await appController.HandleSns(new SnsMessage<CloudFormationRequest<Application>>
            {
                Message = new CloudFormationRequest<Application>
                {
                    ResponseURL = responseUrl,
                    StackId = stackId,
                    RequestId = requestId,
                    LogicalResourceId = logicalResourceId,
                    PhysicalResourceId = physicalResourceId,
                    RequestType = CloudFormationRequestType.Create,
                }
            });

            await func.Should().NotThrowAsync();
            httpContext.ShouldHaveCalled(responseUrl.ToString())
                .WithVerb(HttpMethod.Put)
                .WithContentType("application/json")
                .WithRequestJson(new CloudFormationResponse
                {
                    Status = CloudFormationResponseStatus.FAILED,
                    StackId = stackId,
                    RequestId = requestId,
                    LogicalResourceId = logicalResourceId,
                    PhysicalResourceId = physicalResourceId,
                    Reason = errorMessage,
                });
        }
    }
}
