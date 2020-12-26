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

namespace Brighid.Identity.Roles
{
    [TestFixture, Category("Unit")]
    public class RoleControllerTests
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
        public class GetByNameTests
        {
            [Test, Auto]
            public async Task ShouldReturnEntityIfItExists(
                string name,
                Role role,
                [Frozen, Substitute] IRoleRepository repository,
                [Target] RoleController controller
            )
            {
                repository.FindByName(Any<string>()).Returns(role);
                SetupHttpContext(controller);

                var response = await controller.GetByName(name);
                var result = response.Result;

                result.As<OkObjectResult>().Value.Should().Be(role);
                await repository.Received().FindByName(Is(name));
            }

            [Test, Auto]
            public async Task ShouldReturnNotFoundIfNotExists(
                string name,
                [Frozen, Substitute] IRoleRepository repository,
                [Target] RoleController controller
            )
            {
                repository.FindByName(Any<string>()).Returns((Role)null!);
                SetupHttpContext(controller);

                var response = await controller.GetByName(name);
                var result = response.Result;

                result.Should().BeOfType<NotFoundResult>();
            }
        }
    }
}
