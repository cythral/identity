using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Applications
{
    [JsonConverter(typeof(ApplicationRoleConverter))]
    public class ApplicationRole : INormalizeable
    {
        public ApplicationRole()
        {
        }

        public Guid ApplicationId { get; set; }

        public Guid RoleId { get; set; }

        [ForeignKey("ApplicationId")]
        public Application Application { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }

        [NotMapped]
        public bool IsNormalized { get; private set; }

        public async Task Normalize(DatabaseContext context, CancellationToken cancellationToken = default)
        {
            IsNormalized = true;
            var entry = context.Entry(this);

            switch (entry.State)
            {
                case EntityState.Unchanged:
                case EntityState.Deleted:
                    break;

                case EntityState.Added:
                case EntityState.Modified:
                case EntityState.Detached:
                default:
                    var query = from appRole in context.ApplicationRoles.AsQueryable()
                                where appRole.ApplicationId == ApplicationId
                                where appRole.Role.Name == Role.Name
                                select appRole;

                    if (await query.AnyAsync(cancellationToken))
                    {
                        entry.State = EntityState.Unchanged;
                    }
                    break;
            }
        }
    }
}
