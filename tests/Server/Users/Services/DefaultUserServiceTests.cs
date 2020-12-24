using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Roles;

using FluentAssertions;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using MySqlConnector;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.Users
{
    public class DefaultUserServiceTests
    {
        public class Create
        {
            [Test, Auto]
            public async Task CreatesUser(
                string username,
                string password,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService userService
            )
            {
                var result = IdentityResult.Success;
                userManager.CreateAsync(Any<User>(), Any<string>()).Returns(result);
                userManager.AddToRoleAsync(Any<User>(), Any<string>()).Returns(result);

                await userService.Create(username, password);

                await userManager.Received().CreateAsync(Is<User>(user => user.UserName == username && user.Email == username), Is(password));
            }

            [Test, Auto]
            public async Task ThrowsIfCreationFails(
                string username,
                string password,
                IdentityError error,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService userService
            )
            {
                var userResult = IdentityResult.Failed(error);
                var roleResult = IdentityResult.Success;
                userManager.CreateAsync(Any<User>(), Any<string>()).Returns(userResult);
                userManager.AddToRoleAsync(Any<User>(), Any<string>()).Returns(roleResult);

                Func<Task> func = async () => await userService.Create(username, password);

                await func.Should().ThrowAsync<CreateUserException>();
            }

            [Test, Auto]
            public async Task AddsUserToDefaultRole(
                string username,
                string password,
                [Frozen, Substitute] IUserRoleService roleService,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService userService
            )
            {
                var result = IdentityResult.Success;
                userManager.CreateAsync(Any<User>(), Any<string>()).Returns(result);

                await userService.Create(username, password);

                await roleService.Received().AddRoleToPrincipal(Is<User>(user => user.Email == username && user.Roles != null), Is(nameof(BuiltInRole.Basic)));
            }

            [Test, Auto]
            public async Task AddsUserToGivenRole(
                string username,
                string password,
                string role,
                [Frozen, Substitute] IUserRoleService roleService,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService userService
            )
            {
                var result = IdentityResult.Success;
                userManager.CreateAsync(Any<User>(), Any<string>()).Returns(result);
                userManager.AddToRoleAsync(Any<User>(), Any<string>()).Returns(result);

                await userService.Create(username, password, role);

                await roleService.Received().AddRoleToPrincipal(Is<User>(user => user.Email == username && user.Roles != null), Is(role));
            }
        }

        public class CreateLogin
        {
            [Test, Auto]
            public async Task ShouldThrowIfUserDoesntExist(
                Guid userId,
                UserLogin loginInfo,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService service
            )
            {
                userManager.FindByIdAsync(Any<string>()).Returns((User)null!);

                Func<Task> func = async () => await service.CreateLogin(userId, loginInfo);

                await func.Should().ThrowAsync<UserNotFoundException>();
                await userManager.Received().FindByIdAsync(Is(userId.ToString()));
            }

            [Test, Auto]
            public async Task ShouldThrowIfUserAlreadyHasLoginProviderRegistered(
                Guid userId,
                User user,
                UserLogin loginInfo,
                [Frozen, Substitute] IUserLoginRepository loginRepository,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService service
            )
            {
                var errorCode = MySqlErrorCode.DuplicateKeyEntry;
                var exception = (MySqlException)Activator.CreateInstance(typeof(MySqlException), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { errorCode, "" }, null, null)!;
                userManager.FindByIdAsync(Any<string>()).Returns(user);
                loginRepository.Add(Any<UserLogin>()).Returns<UserLogin>(x =>
                    throw new DbUpdateException("", exception)
                );

                Func<Task> func = async () => await service.CreateLogin(userId, loginInfo);

                await func.Should().ThrowAsync<UserLoginAlreadyExistsException>();
            }

            [Test, Auto]
            public async Task ShouldSaveLoginInfo(
                Guid userId,
                User user,
                UserLogin loginInfo,
                [Frozen, Substitute] IUserLoginRepository loginRepository,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService service
            )
            {
                userManager.FindByIdAsync(Any<string>()).Returns(user);

                await service.CreateLogin(userId, loginInfo);

                await loginRepository.Received().Add(Is<UserLogin>(login =>
                    login.Id == loginInfo.Id &&
                    login.User == user
                ));
            }

            [Test, Auto]
            public async Task ShouldReturnTheSavedLogin(
                Guid userId,
                User user,
                UserLogin loginInfo,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService service
            )
            {
                userManager.FindByIdAsync(Any<string>()).Returns(user);

                var result = await service.CreateLogin(userId, loginInfo);

                result.Should().Be(loginInfo);
            }
        }
    }
}
