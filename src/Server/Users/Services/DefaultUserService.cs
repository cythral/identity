using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Applications;
using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Brighid.Identity.Users
{
#pragma warning disable IDE0059
    public class DefaultUserService : IUserService
    {
        private const string defaultRole = "Basic";
        private readonly UserManager<User> userManager;

        public DefaultUserService(
            UserManager<User> userManager
        )
        {
            this.userManager = userManager;
        }

        public async Task<User> Create(string username, string password, string? role = null)
        {
            static void EnsureSuccess(IdentityResult result)
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
            var user = new User { UserName = username, Email = username };
            user.Roles = new List<UserRole> { new UserRole(user, role) };

            var createResult = await userManager.CreateAsync(user, password);
            EnsureSuccess(createResult);

            return user;
        }
    }
}
