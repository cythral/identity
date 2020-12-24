using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Users
{
    public class DefaultUserRoleRepository : Repository<UserRole, Guid>, IUserRoleRepository
    {
        public DefaultUserRoleRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<Role>> FindRolesForUser(Guid userId, CancellationToken cancellationToken = default)
        {
            var collection = All
            .Include(entity => entity.Role);

            var query = from userRole in collection where userRole.UserId == userId select userRole.Role;
            await query.LoadAsync(cancellationToken);
            return query;
        }
    }
}
