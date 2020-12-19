using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationRepository : Repository<Application, Guid>, IApplicationRepository
    {
        public DefaultApplicationRepository(DatabaseContext context) : base(context) { }

        public async Task<Application?> FindByName(string name, params string[] embeds)
        {
            var collection = embeds.Aggregate(All, (query, embed) => query.Include(embed));
            return await (from app in collection where app.Name == name select app).FirstOrDefaultAsync();
        }
    }
}
