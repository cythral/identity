using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Roles;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

#pragma warning disable CA1040

namespace Brighid.Identity.Applications
{
    [TestFixture]
    [Category("Unit")]
    public class ApplicationControllerTests
    {
        public static HttpContext SetupHttpContext(ControllerBase controller)
        {
            var itemDictionary = new Dictionary<object, object?>();
            var httpContext = Substitute.For<HttpContext>();
            var controllerContext = new ControllerContext { HttpContext = httpContext };
            controller.ControllerContext = controllerContext;
            httpContext.Items.Returns(itemDictionary);
            return httpContext;
        }

        [TestFixture]
        [Category("Unit")]
        public class CreateTests
        {
            [Test]
            [Auto]
            public async Task ShouldThrowIfRoleValidationFails(
                Guid id,
                ApplicationRequest request,
                Application mappedRequest,
                Application application,
                [Frozen, Substitute] IRoleService roleService,
                [Frozen, Substitute] IApplicationMapper mapper,
                [Frozen, Substitute] IApplicationService service,
                [Target] ApplicationController controller
            )
            {
                mapper.MapRequestToEntity(Any<ApplicationRequest>(), Any<CancellationToken>()).Returns(mappedRequest);
                service.GetPrimaryKey(Any<Application>()).Returns(id);
                service.Create(Any<Application>()).Returns(application);
                roleService.When(svc => svc.ValidateRoleDelegations(Any<IEnumerable<string>>(), Any<ClaimsPrincipal>())).Throw(new RoleDelegationDeniedException("Not allowed"));
                SetupHttpContext(controller);

                Func<Task> func = () => controller.Create(request);
                await func.Should().ThrowAsync<RoleDelegationDeniedException>();

                roleService.Received().ValidateRoleDelegations(Is(request.Roles), Is(controller.HttpContext.User));
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class UpdateByIdTests
        {
            [Test]
            [Auto]
            public async Task ShouldReturnForbidIfRoleValidationFails(
                Guid id,
                ApplicationRequest request,
                Application mappedRequest,
                Application application,
                [Frozen, Substitute] IRoleService roleService,
                [Frozen, Substitute] IApplicationMapper mapper,
                [Frozen, Substitute] IApplicationService service,
                [Target] ApplicationController controller
            )
            {
                mapper.MapRequestToEntity(Any<ApplicationRequest>(), Any<CancellationToken>()).Returns(mappedRequest);
                service.GetPrimaryKey(Any<Application>()).Returns(id);
                service.Create(Any<Application>()).Returns(application);
                roleService.When(svc => svc.ValidateRoleDelegations(Any<IEnumerable<string>>(), Any<ClaimsPrincipal>())).Throw(new RoleDelegationDeniedException("Not allowed"));
                SetupHttpContext(controller);

                Func<Task> func = () => controller.UpdateById(id, request);
                await func.Should().ThrowAsync<RoleDelegationDeniedException>();

                roleService.Received().ValidateRoleDelegations(Is(request.Roles), Is(controller.HttpContext.User));
            }
        }
    }
}
