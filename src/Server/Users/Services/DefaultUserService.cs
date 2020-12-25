using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using MySqlConnector;

namespace Brighid.Identity.Users
{
#pragma warning disable IDE0059
    public class DefaultUserService : IUserService
    {
        private const string defaultRole = nameof(BuiltInRole.Basic);
        private readonly UserManager<User> userManager;
        private readonly IUserLoginRepository loginRepository;
        private readonly IUserRoleService roleService;

        public DefaultUserService(
            UserManager<User> userManager,
            IUserLoginRepository loginRepository,
            IUserRoleService roleService
        )
        {
            this.userManager = userManager;
            this.loginRepository = loginRepository;
            this.roleService = roleService;
        }

        public async Task<User> Create(string username, string password, string? role = null)
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
            role ??= defaultRole;

            var user = new User { UserName = username, Email = username, Roles = new List<UserRole>() };
            await roleService.AddRoleToPrincipal(user, role);

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
