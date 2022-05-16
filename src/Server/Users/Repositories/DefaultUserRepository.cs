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
        private static readonly Func<DatabaseContext, string, string, IAsyncEnumerable<User?>> FindByLoginQuery = EF.CompileAsyncQuery<DatabaseContext, string, string, User?>(
            (context, loginProvider, providerKey) =>
                from user in context.Set<User>()
                                     .Include(user => user.Logins)
                                     .Include(user => user.Roles)
                from login in user.Logins
                where login.LoginProvider == loginProvider && login.ProviderKey == providerKey && login.Enabled
                select user
        );

        public DefaultUserRepository(DatabaseContext context)
            : base(context)
        {
        }

        public async Task<User?> FindByLogin(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return await FindByLoginQuery(Context, loginProvider, providerKey).FirstOrDefaultAsync(cancellationToken);
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
