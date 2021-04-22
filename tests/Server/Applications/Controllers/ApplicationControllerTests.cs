using System;
using System.Collections.Generic;
using System.Threading;
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

#pragma warning disable CA1040

namespace Brighid.Identity.Applications
{
    [TestFixture, Category("Unit")]
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

        [TestFixture, Category("Unit")]
        public class CreateTests
        {

            [Test, Auto]
            public async Task ShouldRemoveUnencryptedSecretFromResult_IfRequestSourceIsSns(
                Guid id,
                string secret,
                ApplicationRequest request,
                Application mappedRequest,
                Application application,
                [Frozen, Substitute] IApplicationMapper mapper,
                [Frozen, Substitute] IApplicationService service,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                application.Secret = secret;
                mapper.MapRequestToEntity(Any<ApplicationRequest>(), Any<CancellationToken>()).Returns(mappedRequest);
                service.GetPrimaryKey(Any<Application>()).Returns(id);
                service.Create(Any<Application>()).Returns(application);
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
                ApplicationRequest request,
                Application mappedRequest,
                Application application,
                [Frozen, Substitute] IApplicationMapper mapper,
                [Frozen, Substitute] IApplicationService service,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                application.Secret = secret;
                mapper.MapRequestToEntity(Any<ApplicationRequest>(), Any<CancellationToken>()).Returns(mappedRequest);
                service.GetPrimaryKey(Any<Application>()).Returns(id);
                service.Create(Any<Application>()).Returns(application);
                SetupHttpContext(controller, IdentityRequestSource.Direct);

                var response = await controller.Create(request);
                var result = response.Result.As<CreatedResult>();
                var resultValue = result.Value.As<Application>();

                resultValue.Secret.Should().Be(secret);
            }
        }

        [TestFixture, Category("Unit")]
        public class UpdateByIdTests
        {

            [Test, Auto]
            public async Task ShouldRemoveUnencryptedSecretFromResult_IfRequestSourceIsSns(
                Guid id,
                string secret,
                ApplicationRequest request,
                Application mappedRequest,
                Application application,
                [Frozen, Substitute] IApplicationMapper mapper,
                [Frozen, Substitute] IApplicationService service,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                application.Secret = secret;
                mapper.MapRequestToEntity(Any<ApplicationRequest>(), Any<CancellationToken>()).Returns(mappedRequest);
                service.GetPrimaryKey(Any<Application>()).Returns(id);
                service.UpdateById(Any<Guid>(), Any<Application>()).Returns(application);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                var response = await controller.UpdateById(id, request);
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
                ApplicationRequest request,
                Application mappedRequest,
                Application application,
                [Frozen, Substitute] IApplicationMapper mapper,
                [Frozen, Substitute] IApplicationService service,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                application.Secret = secret;
                mapper.MapRequestToEntity(Any<ApplicationRequest>(), Any<CancellationToken>()).Returns(mappedRequest);
                service.GetPrimaryKey(Any<Application>()).Returns(id);
                service.UpdateById(Any<Guid>(), Any<Application>()).Returns(application);
                SetupHttpContext(controller, IdentityRequestSource.Direct);

                var response = await controller.UpdateById(id, request);
                var result = response.Result.As<OkObjectResult>();
                var resultValue = result.Value.As<Application>();

                resultValue.Secret.Should().Be(secret);
            }
        }
    }
}

