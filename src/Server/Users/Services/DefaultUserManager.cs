using System;
using System.Collections.Generic;
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
    public class DefaultUserManager : UserManager<User>
    {
        public DefaultUserManager(
            IUserStore<User> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<User> passwordHasher,
            IEnumerable<IUserValidator<User>> userValidators,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<User>> logger
        ) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {

        }

        public override Task<string> GetUserIdAsync(User user)
        {
            return Task.FromResult(user.Id.ToString());
        }
    }
}
