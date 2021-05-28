using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

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
                repository.FindById(Any<Guid>(), Any<string[]>()).Returns((User)null!);

                var response = await controller.Get(id);
                var result = response.Result;

                result.Should().BeOfType<NotFoundResult>();
                await repository.Received().FindById(Is(id), Any<string[]>());
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
                repository.FindById(Any<Guid>(), Any<string[]>()).Returns(user);

                var response = await controller.Get(id);
                var result = response.Result;

                result.Should().BeOfType<OkObjectResult>();
                result.As<OkObjectResult>().Value.Should().Be(user);
                await repository.Received().FindById(Is(id), Any<string[]>());
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
                UserLogin loginInfo,
                [Frozen, Substitute] IUserService userService,
                [Target] UserController controller
            )
            {
                userService.CreateLogin(Any<Guid>(), Any<UserLogin>()).Returns<UserLogin>(x => throw new UserNotFoundException(id));

                var response = await controller.CreateLogin(id, loginInfo);
                var result = response.Result;

                result.Should().BeOfType<NotFoundObjectResult>();
                await userService.Received().CreateLogin(Is(id), Is(loginInfo));
            }

            [Test]
            [Auto]
            public async Task ShouldReturnConflictIfLoginAlreadyExists(
                Guid id,
                UserLogin loginInfo,
                [Frozen, Substitute] IUserService userService,
                [Target] UserController controller
            )
            {
                userService.CreateLogin(Any<Guid>(), Any<UserLogin>()).Returns<UserLogin>(x => throw new UserLoginAlreadyExistsException(loginInfo));

                var response = await controller.CreateLogin(id, loginInfo);
                var result = response.Result;

                result.Should().BeOfType<ConflictObjectResult>();
                await userService.Received().CreateLogin(Is(id), Is(loginInfo));
            }

            [Test]
            [Auto]
            public async Task ShouldReturnBadRequestIfModelStateIsInvalid(
                string error1,
                string error2,
                Guid id,
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

                var response = await controller.CreateLogin(id, loginInfo);
                var result = response.Result;

                result.Should().BeOfType<BadRequestObjectResult>();

                dynamic value = result.As<ObjectResult>().Value;
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

                var response = await controller.CreateLogin(id, loginInfo);
                var result = response.Result;

                result.Should().BeOfType<BadRequestObjectResult>();

                dynamic value = result.As<ObjectResult>().Value;
                var errors = (IEnumerable<string>)value.Errors;
                errors.Should().Contain(nonUserError);
                errors.Should().NotContain(userError);
            }

            [Test]
            [Auto]
            public async Task ShouldReturnOkIfUserUserExists(
                Guid id,
                UserLogin requestedLoginInfo,
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
