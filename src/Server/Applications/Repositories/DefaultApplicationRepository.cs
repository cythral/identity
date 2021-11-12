using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationRepository : Repository<Application, Guid>, IApplicationRepository
    {
        public DefaultApplicationRepository(DatabaseContext context)
            : base(context)
        {
        }

        public async Task<Application?> FindByName(string name, params string[] embeds)
        {
            var collection = embeds.Aggregate(All, (query, embed) => query.Include(embed));
            return await (from app in collection where app.Name == name select app).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Role>?> FindRolesById(Guid applicationId, params string[] embeds)
        {
            var collection = embeds.Aggregate(All, (query, embed) => query.Include(embed));
            return await (from app in collection where app.Id == applicationId select app.Roles).FirstOrDefaultAsync();
        }
    }
}
