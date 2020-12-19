using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Sns;

using FluentAssertions;

using Flurl.Http.Testing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.Applications
{
    public class ApplicationControllerTests
    {
        public class CreateTests
        {
            [Test, Auto]
            public async Task ShouldCreateAndReturnApplication(
                Application application,
                HttpContext httpContext,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                appService.Create(Any<Application>()).Returns(application);

                var controllerContext = new ControllerContext { HttpContext = httpContext };
                controller.ControllerContext = controllerContext;

                var response = await controller.Create(application);
                var result = response.Result;

                result.As<CreatedResult>().Value.Should().Be(application);
                await appService.Received().Create(Is(application));
            }

            [Test, Auto]
            public async Task ShouldRedirectToApplicationPage(
                Guid id,
                Application application,
                HttpContext httpContext,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                appService.Create(Any<Application>()).Returns(application);

                var controllerContext = new ControllerContext { HttpContext = httpContext };
                controller.ControllerContext = controllerContext;

                var response = await controller.Create(application);
                var result = response.Result;

                result.As<CreatedResult>().Location.Should().Be($"/api/applications/{id}");
            }

            [Test, Auto]
            public async Task ShouldSetIdItemInHttpContext(
                Guid id,
                Application application,
                HttpContext httpContext,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                appService.Create(Any<Application>()).Returns(application);

                var controllerContext = new ControllerContext { HttpContext = httpContext };
                controller.ControllerContext = controllerContext;

                await controller.Create(application);

                httpContext.Items["identity:id"].Should().Be(id);
            }

            [Test, Auto]
            public async Task ShouldSetModelItemInHttpContext(
                Guid id,
                Application application,
                HttpContext httpContext,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                appService.Create(Any<Application>()).Returns(application);

                var controllerContext = new ControllerContext { HttpContext = httpContext };
                controller.ControllerContext = controllerContext;

                await controller.Create(application);

                httpContext.Items["identity:model"].Should().Be(application);
            }
        }

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
                var response = await controller.Get(id);
                var result = response.Result;

                result.Should().BeOfType<NotFoundResult>();
            }
        }

        public class UpdateTests
        {
            [Test, Auto]
            public async Task ShouldUpdateAndReturnApplication(
                Guid id,
                Application application,
                HttpContext httpContext,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                appService.Update(Any<Guid>(), Any<Application>()).Returns(application);

                var controllerContext = new ControllerContext { HttpContext = httpContext };
                controller.ControllerContext = controllerContext;

                var response = await controller.Update(id, application);
                var result = response.Result;

                result.As<OkObjectResult>().Value.Should().Be(application);
                await appService.Received().Update(Is(id), Is(application));
            }

            [Test, Auto]
            public async Task ShouldSetIdItemInHttpContext(
                Guid id,
                Application application,
                HttpContext httpContext,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                appService.Update(Any<Guid>(), Any<Application>()).Returns(application);

                var controllerContext = new ControllerContext { HttpContext = httpContext };
                controller.ControllerContext = controllerContext;

                await controller.Update(id, application);

                httpContext.Items["identity:id"].Should().Be(id);
            }

            [Test, Auto]
            public async Task ShouldSetModelItemInHttpContext(
                Guid id,
                Application application,
                HttpContext httpContext,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                appService.Update(Any<Guid>(), Any<Application>()).Returns(application);

                var controllerContext = new ControllerContext { HttpContext = httpContext };
                controller.ControllerContext = controllerContext;

                await controller.Update(id, application);

                httpContext.Items["identity:model"].Should().Be(application);
            }
        }

        public class DeleteTests
        {
            [Test, Auto]
            public async Task ShouldUpdateAndReturnApplication(
                Guid id,
                Application application,
                HttpContext httpContext,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                appService.Delete(Any<Guid>()).Returns(application);

                var controllerContext = new ControllerContext { HttpContext = httpContext };
                controller.ControllerContext = controllerContext;

                var response = await controller.Delete(id);
                var result = response.Result;

                result.As<OkObjectResult>().Value.Should().Be(application);
                await appService.Received().Delete(Is(id));
            }

            [Test, Auto]
            public async Task ShouldSetIdItemInHttpContext(
                Guid id,
                Application application,
                HttpContext httpContext,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                appService.Delete(Any<Guid>()).Returns(application);

                var controllerContext = new ControllerContext { HttpContext = httpContext };
                controller.ControllerContext = controllerContext;

                await controller.Delete(id);

                httpContext.Items["identity:id"].Should().Be(id);
            }

            [Test, Auto]
            public async Task ShouldSetModelItemInHttpContext(
                Guid id,
                Application application,
                HttpContext httpContext,
                [Frozen, Substitute] IApplicationService appService,
                [Target] ApplicationController controller
            )
            {
                application.Id = id;
                appService.Delete(Any<Guid>()).Returns(application);

                var controllerContext = new ControllerContext { HttpContext = httpContext };
                controller.ControllerContext = controllerContext;

                await controller.Delete(id);

                httpContext.Items["identity:model"].Should().Be(application);
            }
        }
    }
}
