using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Users;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.LoginProviders
{
    [Category("Unit")]
    public class LoginProviderControllerTests
    {
        [Category("Unit")]
        public class GetUserByLoginProviderTests
        {
            [Test]
            [Auto]
            public void ShouldReturnNotFound_IfUserDoesntExist(
                [Target] LoginProviderController controller
            )
            {
                var mappings = controller.GetExceptionMappings(nameof(LoginProviderController.GetUserByLoginProviderKey));
                mappings.Should().Contain(mapping => mapping.Exception == typeof(UserLoginNotFoundException) && mapping.StatusCode == (int)HttpStatusCode.NotFound);
            }

            [Test]
            [Auto]
            public async Task ShouldReturnOk_IfUserExists(
                string loginProvider,
                string providerKey,
                HttpContext httpContext,
                [Frozen] User user,
                [Frozen, Substitute] IUserService service,
                [Target] LoginProviderController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var response = await controller.GetUserByLoginProviderKey(loginProvider, providerKey);
                var result = response.Result;

                result.Should().BeOfType<OkObjectResult>();
                result.As<OkObjectResult>().Value.Should().Be(user);

                await service.Received().GetByLoginProviderKey(Is(loginProvider), Is(providerKey), Any<CancellationToken>());
            }
        }

        [Category("Unit")]
        public class SetLoginStatus
        {
            [Test]
            [Auto]
            public async Task ShouldSetTheLoginStatus(
                string loginProvider,
                string providerKey,
                bool enabled,
                HttpContext httpContext,
                [Frozen, Substitute] IUserService service,
                [Target] LoginProviderController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                await controller.SetLoginStatus(loginProvider, providerKey, enabled);

                await service.Received().SetLoginStatus(Is(httpContext.User), Is(loginProvider), Is(providerKey), Is(enabled), Is(httpContext.RequestAborted));
            }

            [Test]
            [Auto]
            public async Task ShouldReturnNoContent(
                string loginProvider,
                string providerKey,
                bool enabled,
                HttpContext httpContext,
                [Target] LoginProviderController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var result = await controller.SetLoginStatus(loginProvider, providerKey, enabled);

                result.Should().BeOfType<NoContentResult>();
            }

            [Test]
            [Auto]
            public void ShouldReturnForbiddenIfSecurityExceptionIsThrown(
                string loginProvider,
                string providerKey,
                bool enabled,
                HttpContext httpContext,
                [Target] LoginProviderController controller
            )
            {
                var mappings = controller.GetExceptionMappings(nameof(LoginProviderController.SetLoginStatus));
                mappings.Should().Contain(mapping => mapping.Exception == typeof(SecurityException) && mapping.StatusCode == (int)HttpStatusCode.Forbidden);
            }

            [Test]
            [Auto]
            public void ShouldReturnNotFoundIfLoginNotFoundIsThrown(
                string loginProvider,
                string providerKey,
                bool enabled,
                HttpContext httpContext,
                [Target] LoginProviderController controller
            )
            {
                var mappings = controller.GetExceptionMappings(nameof(LoginProviderController.SetLoginStatus));
                mappings.Should().Contain(mapping => mapping.Exception == typeof(UserLoginNotFoundException) && mapping.StatusCode == (int)HttpStatusCode.NotFound);
            }

            [Test]
            [Auto]
            public void ShouldReturnBadRequestIfInvalidPrincipalIsThrown(
                string loginProvider,
                string providerKey,
                bool enabled,
                HttpContext httpContext,
                [Target] LoginProviderController controller
            )
            {
                var mappings = controller.GetExceptionMappings(nameof(LoginProviderController.SetLoginStatus));
                mappings.Should().Contain(mapping => mapping.Exception == typeof(InvalidPrincipalException) && mapping.StatusCode == (int)HttpStatusCode.BadRequest);
            }
        }
    }
}
