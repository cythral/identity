using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.Roles
{
    [TestFixture]
    [Category("Unit")]
    public class RoleControllerTests
    {
        public static HttpContext SetupHttpContext(Controller controller)
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
        public class ListTests
        {
            [Test]
            [Auto]
            public async Task ShouldReturnListOfRoles(
                IEnumerable<Role> roles,
                [Frozen, Substitute] IRoleService service,
                [Target] RoleController controller
            )
            {
                service.List(Any<CancellationToken>()).Returns(roles);
                SetupHttpContext(controller);

                var response = await controller.List();
                var result = response.Result;

                result.Should().BeOfType<OkObjectResult>();
                result.As<OkObjectResult>().Value.Should().Be(roles);
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class GetByNameTests
        {
            [Test]
            [Auto]
            public async Task ShouldReturnEntityIfItExists(
                string name,
                Role role,
                [Frozen, Substitute] IRoleService service,
                [Target] RoleController controller
            )
            {
                service.GetByName(Any<string>(), Any<CancellationToken>()).Returns(role);
                SetupHttpContext(controller);

                var response = await controller.GetByName(name);
                var result = response.Result;

                result.As<OkObjectResult>().Value.Should().Be(role);
                await service.Received().GetByName(Is(name), Any<CancellationToken>());
            }

            [Test]
            [Auto]
            public async Task ShouldReturnNotFoundIfNotExists(
                string name,
                [Frozen, Substitute] IRoleService service,
                [Target] RoleController controller
            )
            {
                service.GetByName(Any<string>(), Any<CancellationToken>()).Returns((Role)null!);
                SetupHttpContext(controller);

                var response = await controller.GetByName(name);
                var result = response.Result;

                result.Should().BeOfType<NotFoundResult>();
            }
        }
    }
}
