using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Users
{
    [JsonConverter(typeof(UserRoleConverter))]
    public class UserRole : IdentityUserRole<Guid>, IPrincipalRoleJoin<User>
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
        User IPrincipalRoleJoin<User>.Principal
        {
            get => User;
            set => User = value;
        }
    }
}
