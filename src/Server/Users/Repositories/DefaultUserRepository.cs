using System;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
