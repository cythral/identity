using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationRoleRepository : Repository<ApplicationRole, ulong>, IApplicationRoleRepository
    {
        public DefaultApplicationRoleRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<Role>> FindRolesForApplication(string applicationName, CancellationToken cancellationToken = default)
        {
            var collection = All
            .Include(entity => entity.Role)
            .Include(entity => entity.Application);

            var query = from appRole in collection where appRole.Application.Name == applicationName select appRole.Role;
            await query.LoadAsync(cancellationToken);
            return query;
        }
    }
}
