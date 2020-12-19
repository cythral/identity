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
    public class UserRole : IdentityUserRole<Guid>
    {
        public override Guid UserId
        {
            get => base.UserId;
            set => base.UserId = (User ??= new User()).Id = value;
        }

        public override Guid RoleId
        {
            get => base.RoleId;
            set => base.RoleId = (Role ??= new Role()).Id = value;
        }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
    }
}
