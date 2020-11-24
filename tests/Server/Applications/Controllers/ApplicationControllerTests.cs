using System;
using System.Net.Http;
using System.Threading.Tasks;

using AutoFixture.NUnit3;

using Brighid.Identity.Sns;

using FluentAssertions;

using Flurl.Http.Testing;

using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using OpenIddict.Abstractions;

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
            OpenIddictApplicationDescriptor clientApp,
            Application application,
            [Frozen] IApplicationService appService,
            [Target] ApplicationController appController
        )
        {
            using var httpContext = new HttpTest();
            httpContext.RespondWith("OK", 200);
            appService.Create(Any<Application>()).Returns(clientApp);
            var request = new CloudFormationRequest<Application>
            {
                ResponseURL = responseUrl,
                StackId = stackId,
                RequestId = requestId,
                LogicalResourceId = logicalResourceId,
                RequestType = CloudFormationRequestType.Create,
                ResourceProperties = application,
            };

            await appController.HandleSns(new SnsMessage<CloudFormationRequest<Application>>
            {
                Message = request,
            });

            await appService.Received().Create(Is(application));
            await appService.DidNotReceive().Update(Any<Application>());
            await appService.DidNotReceive().Delete(Any<Application>());

            httpContext
            .ShouldHaveCalled(responseUrl.ToString())
            .WithVerb(HttpMethod.Put)
            .WithRequestJson(new CloudFormationResponse(request, application.Name)
            {
                Status = CloudFormationResponseStatus.SUCCESS,
                Data = clientApp,
            });
        }

        [Test, Auto]
        public async Task HandleSns_CallsUpdate(
            string stackId,
            string requestId,
            string logicalResourceId,
            string physicalResourceId,
            string name,
            Uri responseUrl,
            OpenIddictApplicationDescriptor clientApp,
            [Frozen] IApplicationService appService,
            [Target] ApplicationController appController
        )
        {
            using var httpContext = new HttpTest();
            httpContext.RespondWith("OK", 200);
            appService.Update(Any<Application>()).Returns(clientApp);

            var oldApplication = new Application { Name = name, Serial = 1 };
            var newApplication = new Application { Name = name, Serial = 2 };
            var request = new CloudFormationRequest<Application>
            {
                ResponseURL = responseUrl,
                StackId = stackId,
                RequestId = requestId,
                LogicalResourceId = logicalResourceId,
                PhysicalResourceId = physicalResourceId,
                RequestType = CloudFormationRequestType.Update,
                ResourceProperties = newApplication,
                OldResourceProperties = oldApplication,
            };

            await appController.HandleSns(new SnsMessage<CloudFormationRequest<Application>>
            {
                Message = request,
            });

            await appService.Received().Update(Is(newApplication));
            await appService.DidNotReceive().Create(Any<Application>());
            await appService.DidNotReceive().Delete(Any<Application>());

            httpContext
            .ShouldHaveCalled(responseUrl.ToString())
            .WithVerb(HttpMethod.Put)
            .WithRequestJson(new CloudFormationResponse(request, physicalResourceId)
            {
                Status = CloudFormationResponseStatus.SUCCESS,
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
            OpenIddictApplicationDescriptor clientApp,
            [Frozen] IApplicationService appService,
            [Target] ApplicationController appController
        )
        {
            using var httpContext = new HttpTest();
            httpContext.RespondWith("OK", 200);
            appService.Create(Any<Application>()).Returns(clientApp);

            var oldApplication = new Application { Name = oldName };
            var newApplication = new Application { Name = newName };
            var request = new CloudFormationRequest<Application>
            {
                ResponseURL = responseUrl,
                StackId = stackId,
                RequestId = requestId,
                LogicalResourceId = logicalResourceId,
                PhysicalResourceId = physicalResourceId,
                RequestType = CloudFormationRequestType.Update,
                ResourceProperties = newApplication,
                OldResourceProperties = oldApplication,
            };

            await appController.HandleSns(new SnsMessage<CloudFormationRequest<Application>>
            {
                Message = request,
            });

            await appService.Received().Create(Is(newApplication));
            await appService.DidNotReceive().Update(Any<Application>());
            await appService.DidNotReceive().Delete(Any<Application>());

            httpContext
            .ShouldHaveCalled(responseUrl.ToString())
            .WithVerb(HttpMethod.Put)
            .WithRequestJson(new CloudFormationResponse(request, newName)
            {
                Status = CloudFormationResponseStatus.SUCCESS,
                Data = clientApp,
            });
        }

        [Test, Auto]
        public async Task HandleSns_CallsDelete_AndResponds(
            string stackId,
            string requestId,
            string logicalResourceId,
            Uri responseUrl,
            OpenIddictApplicationDescriptor clientApp,
            Application application,
            [Frozen] IApplicationService appService,
            [Target] ApplicationController appController
        )
        {
            using var httpContext = new HttpTest();
            httpContext.RespondWith("OK", 200);
            appService.Delete(Any<Application>()).Returns(clientApp);

            var request = new CloudFormationRequest<Application>
            {
                StackId = stackId,
                RequestId = requestId,
                LogicalResourceId = logicalResourceId,
                ResponseURL = responseUrl,
                RequestType = CloudFormationRequestType.Delete,
                ResourceProperties = application,
            };

            await appController.HandleSns(new SnsMessage<CloudFormationRequest<Application>>
            {
                Message = request,
            });

            await appService.Received().Delete(Is(application));
            await appService.DidNotReceive().Create(Any<Application>());
            await appService.DidNotReceive().Update(Any<Application>());

            httpContext
            .ShouldHaveCalled(responseUrl.ToString())
            .WithVerb(HttpMethod.Put)
            .WithRequestJson(new CloudFormationResponse(request, application.Name)
            {
                Status = CloudFormationResponseStatus.SUCCESS,
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
        public async Task HandleSns_ThrowsIfResourcePropertiesAreNull(
            string stackId,
            string requestId,
            string logicalResourceId,
            string physicalResourceId,
            Uri responseUrl,
            [Target] ApplicationController appController
        )
        {
            using var httpContext = new HttpTest();
            httpContext.RespondWith("OK", 200);

            var request = new CloudFormationRequest<Application>
            {
                ResponseURL = responseUrl,
                StackId = stackId,
                RequestId = requestId,
                LogicalResourceId = logicalResourceId,
                PhysicalResourceId = physicalResourceId,
                RequestType = CloudFormationRequestType.Create,
            };

            Func<Task> func = async () => await appController.HandleSns(new SnsMessage<CloudFormationRequest<Application>>
            {
                Message = request,
            });

            await func.Should().NotThrowAsync();
            httpContext.ShouldHaveCalled(responseUrl.ToString())
                .WithVerb(HttpMethod.Put)
                .WithContentType("application/json")
                .WithRequestJson(new CloudFormationResponse(request)
                {
                    Status = CloudFormationResponseStatus.FAILED,
                    Reason = "Application properties must be specified.",
                });
        }

        [Test, Auto]
        public async Task HandleSns_CatchesExceptionsAndResponds(
            string stackId,
            string requestId,
            string logicalResourceId,
            string physicalResourceId,
            string errorMessage,
            Uri responseUrl,
            Application application,
            [Frozen] IApplicationService appService,
            [Target] ApplicationController appController
        )
        {
            using var httpContext = new HttpTest();
            httpContext.RespondWith("OK", 200);

            appService.Create(Any<Application>()).Returns<OpenIddictApplicationDescriptor>(x => throw new Exception(errorMessage));

            var request = new CloudFormationRequest<Application>
            {
                ResponseURL = responseUrl,
                StackId = stackId,
                RequestId = requestId,
                LogicalResourceId = logicalResourceId,
                PhysicalResourceId = physicalResourceId,
                RequestType = CloudFormationRequestType.Create,
                ResourceProperties = application,
            };

            Func<Task> func = async () => await appController.HandleSns(new SnsMessage<CloudFormationRequest<Application>>
            {
                Message = request,
            });

            await func.Should().NotThrowAsync();
            httpContext.ShouldHaveCalled(responseUrl.ToString())
                .WithVerb(HttpMethod.Put)
                .WithContentType("application/json")
                .WithRequestJson(new CloudFormationResponse(request)
                {
                    Status = CloudFormationResponseStatus.FAILED,
                    Reason = errorMessage,
                });
        }
    }
}
