using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Roles
{
    public class DefaultRoleRepository : Repository<Role, Guid>, IRoleRepository
    {
        public DefaultRoleRepository(DatabaseContext context) : base(context) { }

        public Task<Role?> FindByName(string name, CancellationToken cancellationToken = default)
        {
            var query = from role in All
                        where role.NormalizedName == name.ToUpper(CultureInfo.InvariantCulture)
                        select role;

            return query.FirstOrDefaultAsync(cancellationToken);
        }
    }
}
