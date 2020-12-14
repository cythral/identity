using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Applications;
using Brighid.Identity.Roles;

using FluentAssertions;

using Microsoft.AspNetCore.Identity;

using NSubstitute;

using NUnit.Framework;

using OpenIddict.Server;

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
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService userService
            )
            {
                var result = IdentityResult.Success;
                userManager.CreateAsync(Any<User>(), Any<string>()).Returns(result);
                userManager.AddToRoleAsync(Any<User>(), Any<string>()).Returns(result);

                await userService.Create(username, password);

                await userManager.Received().AddToRoleAsync(Is<User>(user => user.UserName == username && user.Email == username), Is("Basic"));
            }

            [Test, Auto]
            public async Task AddsUserToGivenRole(
                string username,
                string password,
                string role,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService userService
            )
            {
                var result = IdentityResult.Success;
                userManager.CreateAsync(Any<User>(), Any<string>()).Returns(result);
                userManager.AddToRoleAsync(Any<User>(), Any<string>()).Returns(result);

                await userService.Create(username, password, role);

                await userManager.Received().AddToRoleAsync(Is<User>(user => user.UserName == username && user.Email == username), Is(role));
            }

            [Test, Auto]
            public async Task ThrowsExceptionIfAddingToRoleFails(
                string username,
                string password,
                string role,
                IdentityError error,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService userService
            )
            {
                var userResult = IdentityResult.Success;
                var roleResult = IdentityResult.Failed(error);
                userManager.CreateAsync(Any<User>(), Any<string>()).Returns(userResult);
                userManager.AddToRoleAsync(Any<User>(), Any<string>()).Returns(roleResult);

                Func<Task> func = async () => await userService.Create(username, password, role);

                await func.Should().ThrowAsync<CreateUserException>();
            }
        }
    }
}
