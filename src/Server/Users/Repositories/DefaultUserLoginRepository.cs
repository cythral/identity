using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Users
{
    public class DefaultUserLoginRepository : Repository<UserLogin, Guid>, IUserLoginRepository
    {
        private static readonly Func<DatabaseContext, string, string, IAsyncEnumerable<UserLogin?>> FindByProviderNameAndKeyQuery = EF.CompileAsyncQuery<DatabaseContext, string, string, UserLogin?>(
            (context, loginProvider, providerKey) =>
                from login in context.Set<UserLogin>()
                where login.LoginProvider == loginProvider && login.ProviderKey == providerKey
                select login
        );

        public DefaultUserLoginRepository(DatabaseContext context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<UserLogin?> FindByProviderNameAndKey(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await FindByProviderNameAndKeyQuery(Context, loginProvider, providerKey).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
