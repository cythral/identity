using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationRoleRepository : Repository<ApplicationRole, Guid>, IApplicationRoleRepository
    {
        public DefaultApplicationRoleRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<Role>> FindRolesForApplication(Guid applicationId, CancellationToken cancellationToken = default)
        {
            var collection = All
            .Include(entity => entity.Role);

            var query = from appRole in collection where appRole.ApplicationId == applicationId select appRole.Role;
            await query.LoadAsync(cancellationToken);
            return query;
        }
    }
}
