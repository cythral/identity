using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

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

                var response = await controller.Get(id);
                var result = response.Result;

                result.Should().BeOfType<NotFoundResult>();
                await repository.Received().FindById(Is(id), Any<CancellationToken>());
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

                await service.Received().SetDebugMode(Is(userId), Is(enabled), Is(httpContext.RequestAborted));
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
            public async Task ShouldReturnNotFoundIfUserNotFoundExceptionIsThrown(
                Guid userId,
                bool enabled,
                HttpContext httpContext,
                [Frozen] IUserService service,
                [Target] UserController controller
            )
            {
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
                service.SetDebugMode(Any<Guid>(), Any<bool>(), Any<CancellationToken>()).Throws(new UserNotFoundException(userId));

                var result = await controller.SetDebugMode(userId, enabled);

                result.Should().BeOfType<NotFoundObjectResult>();
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
            public async Task ShouldReturnNotFoundIfUserDoesntExist(
                Guid id,
                CreateUserLoginRequest request,
                [Frozen, Substitute] IUserService userService,
                [Target] UserController controller
            )
            {
                userService.CreateLogin(Any<Guid>(), Any<UserLogin>()).Returns<UserLogin>(x => throw new UserNotFoundException(id));

                var response = await controller.CreateLogin(id, request);
                var result = response.Result;

                result.Should().BeOfType<NotFoundObjectResult>();
                await userService.Received().CreateLogin(Is(id), Is(request));
            }

            [Test]
            [Auto]
            public async Task ShouldReturnConflictIfLoginAlreadyExists(
                Guid id,
                CreateUserLoginRequest request,
                [Frozen, Substitute] IUserService userService,
                [Target] UserController controller
            )
            {
                userService.CreateLogin(Any<Guid>(), Any<UserLogin>()).Returns<UserLogin>(x => throw new UserLoginAlreadyExistsException(request));

                var response = await controller.CreateLogin(id, request);
                var result = response.Result;

                result.Should().BeOfType<ConflictObjectResult>();
                await userService.Received().CreateLogin(Is(id), Is(request));
            }

            [Test]
            [Auto]
            public async Task ShouldReturnBadRequestIfModelStateIsInvalid(
                string error1,
                string error2,
                Guid id,
                CreateUserLoginRequest request,
                UserLogin loginInfo,
                [Frozen, Substitute] IUserService userService,
                [Target] UserController controller
            )
            {
                userService.CreateLogin(Any<Guid>(), Any<UserLogin>()).Returns(loginInfo);

                var controllerContext = new ControllerContext();
                controllerContext.ModelState.AddModelError(string.Empty, error1);
                controllerContext.ModelState.AddModelError(string.Empty, error2);
                controller.ControllerContext = controllerContext;

                var response = await controller.CreateLogin(id, request);
                var result = response.Result;

                result.Should().BeOfType<BadRequestObjectResult>();

                dynamic value = result.As<ObjectResult>().Value!;
                var errors = (IEnumerable<string>)value.Errors;
                errors.Should().Contain(error1);
                errors.Should().Contain(error2);
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
                userService.CreateLogin(Any<Guid>(), Any<UserLogin>()).Returns(loginInfo);

                var controllerContext = new ControllerContext();
                controllerContext.ModelState.AddModelError(string.Empty, nonUserError);
                controllerContext.ModelState.AddModelError(nameof(UserLogin.User), userError);
                controller.ControllerContext = controllerContext;

                Console.WriteLine(controller.ControllerContext.ModelState.IsValid);
                var response = await controller.CreateLogin(id, request);
                var result = response.Result;

                result.Should().BeOfType<BadRequestObjectResult>();

                dynamic value = result.As<ObjectResult>().Value!;
                var errors = (IEnumerable<string>)value.Errors;
                errors.Should().Contain(nonUserError);
                errors.Should().NotContain(userError);
            }

            [Test]
            [Auto]
            public async Task ShouldReturnOkIfUserUserExists(
                Guid id,
                CreateUserLoginRequest requestedLoginInfo,
                UserLogin resultingLoginInfo,
                [Frozen, Substitute] IUserService userService,
                [Target] UserController controller
            )
            {
                userService.CreateLogin(Any<Guid>(), Any<UserLogin>()).Returns(resultingLoginInfo);

                var response = await controller.CreateLogin(id, requestedLoginInfo);
                var result = response.Result;

                result.Should().BeOfType<OkObjectResult>();
                result.As<OkObjectResult>().Value.Should().Be(resultingLoginInfo);
                await userService.Received().CreateLogin(Is(id), Is(requestedLoginInfo));
            }
        }
    }
}
