using System;
using System.Linq;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using MySqlConnector;

namespace Brighid.Identity.Users
{
    public class DefaultUserService : IUserService
    {
        private const string DefaultRole = nameof(BuiltInRole.Basic);
        private readonly UserManager<User> userManager;
        private readonly IRoleRepository roleRepository;
        private readonly IUserLoginRepository loginRepository;

        public DefaultUserService(
            UserManager<User> userManager,
            IRoleRepository roleRepository,
            IUserLoginRepository loginRepository
        )
        {
            this.userManager = userManager;
            this.roleRepository = roleRepository;
            this.loginRepository = loginRepository;
        }

        public async Task<User> Create(string username, string password, string? roleName = null)
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

            var role = await roleRepository.FindByName(roleName);
            if (role == null)
            {
                throw new RoleNotFoundException(roleName);
            }

            var user = new User { UserName = username, Email = username };
            user.Roles.Add(role);

            var createResult = await userManager.CreateAsync(user, password);
            EnsureSucceeded(createResult);

            return user;
        }

        public async Task<UserLogin> CreateLogin(Guid userId, UserLogin loginInfo)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

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
    }
}
