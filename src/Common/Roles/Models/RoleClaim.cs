using System;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Roles
{
    public class RoleClaim : IdentityRoleClaim<Guid>
    {
        public override Guid RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}
