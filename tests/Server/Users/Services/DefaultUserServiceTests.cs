using System;
using System.Reflection;
using System.Security;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using MySqlConnector;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.Users
{
    [Category("Unit")]
    public class DefaultUserServiceTests
    {
        [Category("Unit")]
        public class Create
        {
            [Test]
            [Auto]
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

            [Test]
            [Auto]
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
        }

        [Category("Unit")]
        public class GetByLoginProviderKey
        {
            [Test]
            [Auto]
            public async Task ShouldReturnUserFromRepository(
                string loginProvider,
                string providerKey,
                [Frozen] User user,
                [Frozen] IUserRepository repository,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                var result = await service.GetByLoginProviderKey(loginProvider, providerKey, cancellationToken);

                result.Should().Be(user);

                await repository.Received().FindByLogin(Is(loginProvider), Is(providerKey), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldThrowIfNoMatchingUserInTheRepository(
                string loginProvider,
                string providerKey,
                [Frozen] IUserRepository repository,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                repository.FindByLogin(Any<string>(), Any<string>(), Any<CancellationToken>()).ReturnsNull();
                Func<Task> func = () => service.GetByLoginProviderKey(loginProvider, providerKey, cancellationToken);
                await func.Should().ThrowAsync<UserLoginNotFoundException>();
            }
        }

        [Category("Unit")]
        public class CreateLogin
        {
            [Test]
            [Auto]
            public async Task ShouldThrowIfUserDoesntExist(
                Guid userId,
                UserLogin loginInfo,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                userManager.FindByIdAsync(Any<string>()).Returns((User)null!);

                Func<Task> func = async () => await service.CreateLogin(userId, loginInfo, cancellationToken);

                await func.Should().ThrowAsync<UserNotFoundException>();
                await userManager.Received().FindByIdAsync(Is(userId.ToString()));
            }

            [Test]
            [Auto]
            public async Task ShouldThrowIfUserAlreadyHasLoginProviderRegistered(
                Guid userId,
                User user,
                UserLogin loginInfo,
                [Frozen, Substitute] IUserLoginRepository loginRepository,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                var errorCode = MySqlErrorCode.DuplicateKeyEntry;
                var exception = (MySqlException)Activator.CreateInstance(typeof(MySqlException), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { errorCode, string.Empty }, null, null)!;
                userManager.FindByIdAsync(Any<string>()).Returns(user);
                loginRepository.Add(Any<UserLogin>()).Returns<UserLogin>(x =>
                    throw new DbUpdateException(string.Empty, exception)
                );

                Func<Task> func = async () => await service.CreateLogin(userId, loginInfo, cancellationToken);

                await func.Should().ThrowAsync<UserLoginAlreadyExistsException>();
            }

            [Test]
            [Auto]
            public async Task ShouldSaveLoginInfo(
                Guid userId,
                User user,
                UserLogin loginInfo,
                [Frozen, Substitute] IUserLoginRepository loginRepository,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                userManager.FindByIdAsync(Any<string>()).Returns(user);

                await service.CreateLogin(userId, loginInfo, cancellationToken);

                await loginRepository.Received().Add(Is<UserLogin>(login =>
                    login.Id == loginInfo.Id &&
                    login.User == user
                ));
            }

            [Test]
            [Auto]
            public async Task ShouldReturnTheSavedLogin(
                Guid userId,
                User user,
                UserLogin loginInfo,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                userManager.FindByIdAsync(Any<string>()).Returns(user);

                var result = await service.CreateLogin(userId, loginInfo, cancellationToken);

                result.Should().Be(loginInfo);
            }
        }

        [Category("Unit")]
        public class SetLoginStatus
        {
            [Test]
            [Auto]
            public async Task ShouldSearchRepositoryForUserLogin(
                ClaimsPrincipal principal,
                string loginProvider,
                string providerKey,
                bool enabled,
                [Frozen] UserLogin userLogin,
                [Frozen] IUserLoginRepository repository,
                [Frozen] IPrincipalService principalService,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                principalService.GetId(Any<ClaimsPrincipal>()).Returns(userLogin.UserId);

                await service.SetLoginStatus(principal, loginProvider, providerKey, enabled, cancellationToken);

                await repository.Received().FindByProviderNameAndKey(loginProvider, providerKey, cancellationToken);
            }

            [Test]
            [Auto]
            public async Task ShouldThrowIfLoginWasNotFound(
                ClaimsPrincipal principal,
                string loginProvider,
                string providerKey,
                bool enabled,
                [Frozen] UserLogin userLogin,
                [Frozen] IPrincipalService principalService,
                [Frozen] IUserLoginRepository repository,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                principalService.GetId(Any<ClaimsPrincipal>()).Returns(userLogin.UserId);
                repository.FindByProviderNameAndKey(Any<string>(), Any<string>(), Any<CancellationToken>()).ReturnsNull();

                Func<Task> func = () => service.SetLoginStatus(principal, loginProvider, providerKey, enabled, cancellationToken);

                var result = (await func.Should().ThrowAsync<UserLoginNotFoundException>()).Which;
                result.LoginProvider.Should().Be(loginProvider);
                result.ProviderKey.Should().Be(providerKey);
            }

            [Test]
            [Auto]
            public async Task ShouldThrowIfLoginProviderDoesNotBelongToTheUser(
                ClaimsPrincipal principal,
                string loginProvider,
                string providerKey,
                bool enabled,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => service.SetLoginStatus(principal, loginProvider, providerKey, enabled, cancellationToken);

                await func.Should().ThrowAsync<SecurityException>();
            }

            [Test]
            [Auto]
            public async Task ShouldDisableTheUserLogin(
                ClaimsPrincipal principal,
                string loginProvider,
                string providerKey,
                bool enabled,
                [Frozen] UserLogin userLogin,
                [Frozen] IPrincipalService principalService,
                [Frozen] IUserLoginRepository repository,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                userLogin.Enabled = !enabled;
                principalService.GetId(Any<ClaimsPrincipal>()).Returns(userLogin.UserId);
                await service.SetLoginStatus(principal, loginProvider, providerKey, enabled, cancellationToken);

                await repository.Received().Save(Is(userLogin), Is(cancellationToken));
                userLogin.Enabled.Should().Be(enabled);
            }
        }

        [Category("Unit")]
        public class SetDebugMode
        {
            [Test]
            [Auto]
            public async Task ShouldTurnOnDebugModeForUsersThatExist(
                Guid userId,
                ClaimsPrincipal principal,
                [Frozen] User user,
                [Frozen] IPrincipalService principalService,
                [Frozen] IUserRepository repository,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                principalService.GetId(Any<ClaimsPrincipal>()).Returns(userId);
                user.Flags = UserFlags.None;

                await service.SetDebugMode(principal, userId, true, cancellationToken);

                await repository.Received().Save(Is<User>(user => user.Flags.HasFlag(UserFlags.Debug)), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldTurnOffDebugModeForUsersThatExist(
                Guid userId,
                ClaimsPrincipal principal,
                [Frozen] User user,
                [Frozen] IPrincipalService principalService,
                [Frozen] IUserRepository repository,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                principalService.GetId(Any<ClaimsPrincipal>()).Returns(userId);
                user.Flags = UserFlags.Debug;

                await service.SetDebugMode(principal, userId, false, cancellationToken);

                await repository.Received().Save(Is<User>(user => !user.Flags.HasFlag(UserFlags.Debug)), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldThrowSecurityExceptionIfIdDoesNotMatchPrincipal(
                Guid userId,
                ClaimsPrincipal principal,
                [Frozen] User user,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                user.Flags = UserFlags.Debug;

                Func<Task> func = () => service.SetDebugMode(principal, userId, false, cancellationToken);

                await func.Should().ThrowAsync<SecurityException>();
            }

            [Test]
            [Auto]
            public async Task ShouldThrowIfUserNotFound(
                Guid userId,
                ClaimsPrincipal principal,
                [Frozen] IPrincipalService principalService,
                [Frozen] IUserRepository repository,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                principalService.GetId(Any<ClaimsPrincipal>()).Returns(userId);
                repository.FindById(Any<Guid>(), Any<CancellationToken>()).Returns(null as User);

                Func<Task> func = () => service.SetDebugMode(principal, userId, true, cancellationToken);

                await func.Should().ThrowAsync<UserNotFoundException>();
            }

            [Test]
            [Auto]
            public async Task ShouldExpireTheUserCacheForTheGivenUser(
                Guid userId,
                ClaimsPrincipal principal,
                [Frozen] IPrincipalService principalService,
                [Frozen] IUserCacheService cacheService,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                principalService.GetId(Any<ClaimsPrincipal>()).Returns(userId);

                await service.SetDebugMode(principal, userId, false, cancellationToken);

                await cacheService.Received().ClearExternalUserCache(Is(userId), Is(cancellationToken));
            }
        }
    }
}
