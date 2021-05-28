using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Users
{
    public class DefaultUserRepository : Repository<User, Guid>, IUserRepository
    {
        public DefaultUserRepository(DatabaseContext context) : base(context) { }

        public async Task<User?> FindByLogin(string loginProvider, string providerKey, params string[] embeds)
        {
            var collection = embeds.Aggregate(All, (query, embed) => query.Include(embed));
            var query = from user in collection
                        from login in user.Logins
                        where login.LoginProvider == loginProvider && login.ProviderKey == providerKey
                        select user;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Role>?> FindRolesById(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var collection = All.Include(user => user.Roles);
            var query = from user in collection
                        where user.Id == id
                        select user.Roles;

            return await query.FirstOrDefaultAsync(cancellationToken);
        }
    }
}
