using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Roles
{
    public class DefaultRoleRepository : Repository<Role, Guid>, IRoleRepository
    {
        public DefaultRoleRepository(DatabaseContext context)
            : base(context)
        {
        }

        public Task<Role?> FindByName(string name, CancellationToken cancellationToken = default)
        {
            var query = from role in All
                        where role.NormalizedName == name.ToUpper(CultureInfo.InvariantCulture)
                        select role;

            return query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> IsAttachedToAPrincipal(Guid id, CancellationToken cancellationToken = default)
        {
            var query = Context.Roles.FromSqlInterpolated($"select 1 from Roles where exists (select 1 from RoleUser where RoleId = {id}) or (select 1 from ApplicationRole where RoleId = {id}) limit 1");
            return await query.AnyAsync(cancellationToken);
        }

        public async Task<IEnumerable<Role>> FindAllByName(IEnumerable<string> names, CancellationToken cancellationToken = default)
        {
            var placeholders = string.Join(",", Enumerable.Range(0, names.Count()).Select(i => "{" + i + "}"));
            var values = names.Cast<object>().ToArray();
            var query = Context.Roles.FromSqlRaw($@"select * from Roles where Name in ({placeholders})", values);
            await query.LoadAsync(cancellationToken);
            return query;
        }
    }
}
