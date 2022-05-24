using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using MySqlConnector;

namespace Brighid.Identity.Users
{
    /// <inheritdoc />
    public class DefaultUserService : IUserService
    {
        private const string DefaultRole = nameof(BuiltInRole.Basic);
        private readonly UserManager<User> userManager;
        private readonly IUserRepository userRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IUserLoginRepository loginRepository;
        private readonly IPrincipalService principalService;
        private readonly IUserCacheService cacheService;

        public DefaultUserService(
            UserManager<User> userManager,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserLoginRepository loginRepository,
            IPrincipalService principalService,
            IUserCacheService cacheService
        )
        {
            this.userManager = userManager;
            this.userRepository = userRepository;
            this.roleRepository = roleRepository;
            this.loginRepository = loginRepository;
            this.principalService = principalService;
            this.cacheService = cacheService;
        }

        /// <inheritdoc />
        public async Task<User> Create(string username, string password, string? roleName = null, CancellationToken cancellationToken = default)
        {
            static void EnsureSucceeded(IdentityResult result)
            {
                if (!result.Succeeded)
                {
                    var innerExceptions = result.Errors
                        .Select(error => new CreateUserException(error.Description))
                        .ToArray();
                    throw new CreateUserException(innerExceptions);
                }
            }

            roleName ??= DefaultRole;

            var role = await roleRepository.FindByName(roleName, cancellationToken) ?? throw new RoleNotFoundException(roleName);
            var user = new User { UserName = username, Email = username };
            user.Roles.Add(role);

            var createResult = await userManager.CreateAsync(user, password);
            EnsureSucceeded(createResult);

            return user;
        }

        /// <inheritdoc />
        public async Task<UserLogin> CreateLogin(Guid userId, UserLogin loginInfo, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new UserNotFoundException(userId);
            loginInfo.User = user;

            try
            {
                await loginRepository.Add(loginInfo);
            }
            catch (DbUpdateException e)
                when ((e.InnerException as MySqlException)?.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
            {
                throw new UserLoginAlreadyExistsException(loginInfo);
            }

            return loginInfo;
        }

        /// <inheritdoc />
        public async Task<User> GetByLoginProviderKey(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var result = await userRepository.FindByLogin(loginProvider, providerKey, cancellationToken) ?? throw new UserLoginNotFoundException(loginProvider, providerKey);
            return result!;
        }

        /// <inheritdoc />
        public async Task SetLoginStatus(ClaimsPrincipal principal, string loginProvider, string providerKey, bool enabled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var login = await loginRepository.FindByProviderNameAndKey(loginProvider, providerKey, cancellationToken) ?? throw new UserLoginNotFoundException(loginProvider, providerKey);
            EnsureUserIdMatches(principal, login.UserId, $"Insufficient permissions for enabling or disabling the user login {loginProvider}/{providerKey}.");

            login.Enabled = enabled;
            await loginRepository.Save(login, cancellationToken);
        }

        /// <inheritdoc />
        public async Task SetDebugMode(ClaimsPrincipal principal, Guid userId, bool enabled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureUserIdMatches(principal, userId, $"Insufficient permissions for setting debug mode for user {userId}");

            var user = await userRepository.FindById(userId, cancellationToken) ?? throw new UserNotFoundException(userId);
            user.Flags = enabled
                ? user.Flags | UserFlags.Debug
                : user.Flags ^ UserFlags.Debug;

            await userRepository.Save(user, cancellationToken);
            await cacheService.ClearExternalUserCache(userId, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureUserIdMatches(ClaimsPrincipal principal, Guid userId, string errorMessage)
        {
            if (userId != principalService.GetId(principal))
            {
                throw new SecurityException(errorMessage);
            }
        }
    }
}
