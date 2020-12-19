using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Roles
{
    public class Role : IdentityRole<Guid>, INormalizeable
    {
        /// <summary>
        /// Gets the ID number for this role.
        /// </summary>
        /// <value>A unique id number.</value>
        [Key]
        public override Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the description of the role.
        /// </summary>
        /// <value>The description of the role.</value>
        public string Description { get; set; } = "";

        public virtual ICollection<RoleClaim> Claims { get; set; } = new List<RoleClaim>();

        [NotMapped]
        public bool IsNormalized { get; private set; }

        public async Task Normalize(DatabaseContext context, CancellationToken cancellationToken = default)
        {
            NormalizedName = Name.ToUpper(CultureInfo.InvariantCulture);
            IsNormalized = true;

            // Only insert/update/delete entities that were loaded from the database
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
                    var query = from role in context.Roles.AsQueryable()
                                where role.NormalizedName == NormalizedName
                                select role.Id;

                    var existingId = await query.FirstOrDefaultAsync(cancellationToken);
                    entry.State = EntityState.Added;

                    if (await query.AnyAsync(cancellationToken) && existingId != Id)
                    {
                        entry.State = EntityState.Unchanged;
                    }
                    break;
            }
        }
    }
}
