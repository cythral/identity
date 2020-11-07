using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationRoleRepository : Repository<ApplicationRole, string>, IApplicationRoleRepository
    {
        public DefaultApplicationRoleRepository(DatabaseContext context) : base(context) { }

        public async Task<IEnumerable<Role>> FindRolesForApplication(string applicationName, CancellationToken cancellationToken = default)
        {
            var query = from appRole in All.Include(entity => entity.Role) where appRole.ApplicationName == applicationName select appRole.Role;
            await query.LoadAsync(cancellationToken);
            return query;
        }
    }
}
