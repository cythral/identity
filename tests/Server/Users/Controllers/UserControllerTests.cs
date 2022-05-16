using System;
using System.Linq;
using System.Net;
using System.Reflection;
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

namespace Brighid.Identity.Users
{
    [Category("Unit")]
    public class UserControllerTests
    {
        [Category("Unit")]
        public class GetTests
        {
            [Test]
            [Auto]
            public async Task ShouldReturnNotFound_IfUserDoesntExist(
                Guid id,
                [Frozen, Substitute] IUserRepository repository,
                [Target] UserController controller
            )
            {
                repository.FindById(Any<Guid>(), Any<CancellationToken>()).Returns((User)null!);

                Func<Task> func = () => controller.Get(id);
                await func.Should().ThrowAsync<UserNotFoundException>();
            }

            [Test]
            [Auto]
            public async Task ShouldReturnOk_IfUserExists(
                Guid id,
                User user,
                [Frozen, Substitute] IUserRepository repository,
                [Target] UserController controller
            )
            {
                repository.FindById(Any<Guid>(), Any<CancellationToken>()).Returns(user);

                var response = await controller.Get(id);
                var result = response.Result;

                result.Should().BeOfType<OkObjectResult>();
                result.As<OkObjectResult>().Value.Should().Be(user);
                await repository.Received().FindById(Is(id), Any<CancellationToken>());
            }
        }

        [Category("Unit")]
        public class SetDebugModeTests
        {
            [Test]
            [Auto]
            public async Task ShouldSetDebugMode(
                Guid userId,
                bool enabled,
                HttpContext httpContext,
                [Frozen] IUserService service,
                [Target] UserController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                await controller.SetDebugMode(userId, enabled);

                await service.Received().SetDebugMode(Is(httpContext.User), Is(userId), Is(enabled), Is(httpContext.RequestAborted));
            }

            [Test]
            [Auto]
            public async Task ShouldReturnNoContentAfterSuccess(
                Guid userId,
                bool enabled,
                HttpContext httpContext,
                [Target] UserController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var result = await controller.SetDebugMode(userId, enabled);

                result.Should().BeOfType<NoContentResult>();
            }

            [Test]
            [Auto]
            public void ShouldReturnNotFoundIfUserNotFoundExceptionIsThrown(
                Guid userId,
                bool enabled,
                HttpContext httpContext,
                [Frozen] IUserService service,
                [Target] UserController controller
            )
            {
                var method = typeof(UserController).GetMethod(nameof(UserController.SetDebugMode))!;
                var attributes = from attr in method.GetCustomAttributes() where attr is IExceptionMapping select (IExceptionMapping)attr;
                attributes.Should().Contain(attribute => attribute.Exception == typeof(UserNotFoundException) && attribute.StatusCode == (int)HttpStatusCode.NotFound);
            }
        }

        [Category("Unit")]
        public class CreateLoginTests
        {
            [Test]
            public void ShouldBeRestrictedToSelf()
            {
                var methodInfo = typeof(UserController).GetMethod(nameof(UserController.CreateLogin))!;
                var attribute = methodInfo.GetCustomAttributes(true).OfType<PoliciesAttribute>().First();

                attribute.Should().NotBeNull();
                attribute!.Policies.Should().Contain(nameof(IdentityPolicy.RestrictedToSelfByUserId));
            }

            [Test]
            public void ShouldBeAccessibleViaPost()
            {
                var methodInfo = typeof(UserController).GetMethod(nameof(UserController.CreateLogin))!;
                var attribute = methodInfo.GetCustomAttributes(true).OfType<HttpPostAttribute>().First();

                attribute.Should().NotBeNull();
                attribute!.Template.Should().Be("{userId}/logins");
            }

            [Test]
            [Auto]
            public void ShouldReturnNotFoundIfUserDoesntExist(
                Guid id,
                CreateUserLoginRequest request,
                [Frozen, Substitute] IUserService userService,
                [Target] UserController controller
            )
            {
                var method = typeof(UserController).GetMethod(nameof(UserController.CreateLogin))!;
                var attributes = from attr in method.GetCustomAttributes() where attr is IExceptionMapping select (IExceptionMapping)attr;
                attributes.Should().Contain(attribute => attribute.Exception == typeof(UserNotFoundException) && attribute.StatusCode == (int)HttpStatusCode.NotFound);
            }

            [Test]
            [Auto]
            public void ShouldReturnConflictIfLoginAlreadyExists(
                Guid id,
                CreateUserLoginRequest request,
                [Frozen, Substitute] IUserService userService,
                [Target] UserController controller
            )
            {
                var method = typeof(UserController).GetMethod(nameof(UserController.CreateLogin))!;
                var attributes = from attr in method.GetCustomAttributes() where attr is IExceptionMapping select (IExceptionMapping)attr;
                attributes.Should().Contain(attribute => attribute.Exception == typeof(UserLoginAlreadyExistsException) && attribute.StatusCode == (int)HttpStatusCode.Conflict);
            }

            [Test]
            [Auto]
            public void ShouldReturnBadRequestIfModelStateIsInvalid(
                string error1,
                string error2,
                Guid id,
                CreateUserLoginRequest request,
                UserLogin loginInfo,
                [Frozen, Substitute] IUserService userService,
                [Target] UserController controller
            )
            {
                var method = typeof(UserController).GetMethod(nameof(UserController.CreateLogin))!;
                var attributes = from attr in method.GetCustomAttributes() where attr is IExceptionMapping select (IExceptionMapping)attr;
                attributes.Should().Contain(attribute => attribute.Exception == typeof(ModelStateException) && attribute.StatusCode == (int)HttpStatusCode.BadRequest);
            }

            [Test]
            [Auto]
            public async Task ShouldRemoveUserPropertyModelStateErrors(
                string nonUserError,
                string userError,
                Guid id,
                CreateUserLoginRequest request,
                UserLogin loginInfo,
                [Frozen, Substitute] IUserService userService,
                [Target] UserController controller
            )
            {
                userService.CreateLogin(Any<Guid>(), Any<UserLogin>(), Any<CancellationToken>()).Returns(loginInfo);

                var controllerContext = new ControllerContext();
                controllerContext.ModelState.AddModelError(string.Empty, nonUserError);
                controllerContext.ModelState.AddModelError(nameof(UserLogin.User), userError);
                controller.ControllerContext = controllerContext;

                Console.WriteLine(controller.ControllerContext.ModelState.IsValid);
                Func<Task> func = () => controller.CreateLogin(id, request);
                await func.Should().ThrowAsync<ModelStateException>();
                controllerContext.ModelState.Should().NotContain(error => error.Key == nameof(UserLogin.User));
            }

            [Test]
            [Auto]
            public async Task ShouldReturnOkIfUserUserExists(
                Guid id,
                CreateUserLoginRequest requestedLoginInfo,
                UserLogin resultingLoginInfo,
                HttpContext httpContext,
                [Frozen, Substitute] IUserService userService,
                [Target] UserController controller
            )
            {
                userService.CreateLogin(Any<Guid>(), Any<UserLogin>(), Any<CancellationToken>()).Returns(resultingLoginInfo);

                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var response = await controller.CreateLogin(id, requestedLoginInfo);
                var result = response.Result;

                result.Should().BeOfType<OkObjectResult>();
                result.As<OkObjectResult>().Value.Should().Be(resultingLoginInfo);
                await userService.Received().CreateLogin(Is(id), Is(requestedLoginInfo), Is(httpContext.RequestAborted));
            }
        }
    }
}
