using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Roles
{
    [JsonConverter(typeof(UserRoleConverter))]
    public class UserRole : IdentityUserRole<Guid>, INormalizeable
    {
        public UserRole() { }

        public UserRole(User user, string roleName)
        {
            User = user;
            UserId = user.Id;
            Role = new Role { Name = roleName };
        }
        public override Guid UserId { get; set; }

        public override Guid RoleId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        [NotMapped]
        public bool IsNormalized { get; private set; }

        public async Task Normalize(DatabaseContext context, CancellationToken cancellationToken = default)
        {
            UserId = User.Id;
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
                    var existingUserRoleQuery = from userRole in context.UserRoles.AsQueryable()
                                                where userRole.UserId == UserId
                                                where userRole.Role.Name == Role.Name
                                                select userRole;

                    if (await existingUserRoleQuery.AnyAsync(cancellationToken))
                    {
                        entry.State = EntityState.Unchanged;
                        return;
                    }

                    var existingRoleQuery = from role in context.Roles.AsQueryable()
                                            where role.Name == Role.Name
                                            select role;

                    if (await existingRoleQuery.AnyAsync(cancellationToken))
                    {
                        Role = await existingRoleQuery.FirstAsync(cancellationToken);
                    }

                    break;
            }
        }
    }
}
