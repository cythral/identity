using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Sns;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.Applications
{
    [Category("Unit")]
    public class ApplicationControllerTests
    {

        public static HttpContext SetupHttpContext(Controller controller, IdentityRequestSource source = IdentityRequestSource.Direct)
        {
            var itemDictionary = new Dictionary<object, object?>();
            var httpContext = Substitute.For<HttpContext>();
            var controllerContext = new ControllerContext { HttpContext = httpContext };
            controller.ControllerContext = controllerContext;
            httpContext.Items.Returns(itemDictionary);
            httpContext.Items[Constants.RequestSource] = source;
            return httpContext;
        }

        [Category("Unit")]
        public class CreateTests
        {
            [Test, Auto]
            public async Task ShouldCreateAndReturnApplication(
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                appService.Create(Any<Application>()).Returns(application);
                SetupHttpContext(controller);

                var response = await controller.Create(application);
                var result = response.Result;

                result.As<CreatedResult>().Value.Should().Be(application);
                await appService.Received().Create(Is(application));
            }

            [Test, Auto]
            public async Task ShouldRedirectToApplicationPage(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                appService.Create(Any<Application>()).Returns(application);
                SetupHttpContext(controller);

                var response = await controller.Create(application);
                var result = response.Result;

                result.As<CreatedResult>().Location.Should().Be($"/api/applications/{id}");
            }

            [Test, Auto]
            public async Task ShouldSetIdItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                appService.Create(Any<Application>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.Create(application);

                httpContext.Items[CloudFormationConstants.Id].Should().Be(id);
            }

            [Test, Auto]
            public async Task ShouldNotSetIdItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                appService.Create(Any<Application>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.Create(application);

                httpContext.Items.Should().NotContainKey(CloudFormationConstants.Id);
            }

            [Test, Auto]
            public async Task ShouldSetDataItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                Application request,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                appService.Create(Any<Application>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.Create(request);

                httpContext.Items[CloudFormationConstants.Data].Should().Be(application);
            }

            [Test, Auto]
            public async Task ShouldNotSetDataItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                appService.Create(Any<Application>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.Create(application);

                httpContext.Items.Should().NotContainKey(CloudFormationConstants.Data);
            }

            [Test, Auto]
            public async Task ShouldRemoveUnencryptedSecretFromResult_IfRequestSourceIsSns(
                Guid id,
                string secret,
                Application request,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                application.Secret = secret;
                appService.Create(Any<Application>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                var response = await controller.Create(request);
                var result = response.Result.As<CreatedResult>();
                var resultValue = result.Value.As<Application>();
                var data = httpContext.Items[CloudFormationConstants.Data];

                data.As<Application>().Secret.Should().BeNull();
                resultValue.Secret.Should().BeNull();
            }

            [Test, Auto]
            public async Task ShouldNotRemoveUnencryptedSecretFromResult_IfRequestSourceIsDirect(
                Guid id,
                string secret,
                Application request,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                application.Secret = secret;
                appService.Create(Any<Application>()).Returns(application);
                SetupHttpContext(controller, IdentityRequestSource.Direct);

                var response = await controller.Create(request);
                var result = response.Result.As<CreatedResult>();
                var resultValue = result.Value.As<Application>();

                resultValue.Secret.Should().Be(secret);
            }
        }

        [Category("Unit")]
        public class GetTests
        {
            [Test, Auto]
            public async Task ShouldReturnEntityIfItExists(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationRepository repository,
                [Target] ApplicationController controller
            )
            {
                repository.GetById(Any<Guid>()).Returns(application);
                SetupHttpContext(controller);

                var response = await controller.Get(id);
                var result = response.Result;

                result.As<OkObjectResult>().Value.Should().Be(application);
                await repository.Received().GetById(Is(id));
            }

            [Test, Auto]
            public async Task ShouldReturnNotFoundIfNotExists(
                Guid id,
                [Frozen, Substitute] IApplicationRepository repository,
                [Target] ApplicationController controller
            )
            {
                repository.GetById(Any<Guid>()).Returns((Application)null!);
                SetupHttpContext(controller);

                var response = await controller.Get(id);
                var result = response.Result;

                result.Should().BeOfType<NotFoundResult>();
            }
        }

        [Category("Unit")]
        public class UpdateTests
        {
            [Test, Auto]
            public async Task ShouldUpdateAndReturnApplication(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                appService.Update(Any<Guid>(), Any<Application>()).Returns(application);
                SetupHttpContext(controller);

                var response = await controller.Update(id, application);
                var result = response.Result;

                result.As<OkObjectResult>().Value.Should().Be(application);
                await appService.Received().Update(Is(id), Is(application));
            }

            [Test, Auto]
            public async Task ShouldSetIdItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                appService.Update(Any<Guid>(), Any<Application>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.Update(id, application);

                httpContext.Items[CloudFormationConstants.Id].Should().Be(id);
            }

            [Test, Auto]
            public async Task ShouldNotSetIdItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                appService.Update(Any<Guid>(), Any<Application>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.Update(id, application);

                httpContext.Items.Should().NotContainKey(CloudFormationConstants.Id);
            }

            [Test, Auto]
            public async Task ShouldSetDataItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                appService.Update(Any<Guid>(), Any<Application>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.Update(id, application);

                httpContext.Items[CloudFormationConstants.Data].Should().Be(application);
            }

            [Test, Auto]
            public async Task ShouldNotSetDataItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                appService.Update(Any<Guid>(), Any<Application>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.Update(id, application);

                httpContext.Items.Should().NotContainKey(CloudFormationConstants.Data);
            }

            [Test, Auto]
            public async Task ShouldRemoveUnencryptedSecretFromResult_IfRequestSourceIsSns(
                Guid id,
                string secret,
                Application request,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                application.Secret = secret;
                appService.Update(Any<Guid>(), Any<Application>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                var response = await controller.Update(id, request);
                var result = response.Result.As<OkObjectResult>();
                var resultValue = result.Value.As<Application>();
                var data = httpContext.Items[CloudFormationConstants.Data];

                data.As<Application>().Secret.Should().BeNull();
                resultValue.Secret.Should().BeNull();
            }

            [Test, Auto]
            public async Task ShouldNotRemoveUnencryptedSecretFromResult_IfRequestSourceIsDirect(
                Guid id,
                string secret,
                Application request,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                application.Secret = secret;
                appService.Update(Any<Guid>(), Any<Application>()).Returns(application);
                SetupHttpContext(controller, IdentityRequestSource.Direct);

                var response = await controller.Update(id, request);
                var result = response.Result.As<OkObjectResult>();
                var resultValue = result.Value.As<Application>();

                resultValue.Secret.Should().Be(secret);
            }
        }

        [Category("Unit")]
        public class DeleteTests
        {
            [Test, Auto]
            public async Task ShouldUpdateAndReturnApplication(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                appService.Delete(Any<Guid>()).Returns(application);
                SetupHttpContext(controller);

                var response = await controller.Delete(id);
                var result = response.Result;

                result.As<OkObjectResult>().Value.Should().Be(application);
                await appService.Received().Delete(Is(id));
            }

            [Test, Auto]
            public async Task ShouldSetIdItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                appService.Delete(Any<Guid>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.Delete(id);

                httpContext.Items[CloudFormationConstants.Id].Should().Be(id);
            }

            [Test, Auto]
            public async Task ShouldNotSetIdItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                appService.Delete(Any<Guid>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.Delete(id);

                httpContext.Items.Should().NotContainKey(CloudFormationConstants.Id);
            }

            [Test, Auto]
            public async Task ShouldSetDataItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                appService.Delete(Any<Guid>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.Delete(id);

                httpContext.Items[CloudFormationConstants.Data].Should().Be(application);
            }

            [Test, Auto]
            public async Task ShouldNotSetDataItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                appService.Delete(Any<Guid>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.Delete(id);

                httpContext.Items.Should().NotContainKey(CloudFormationConstants.Data);
            }
        }
    }
}
