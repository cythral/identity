using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Roles
{
    public class RoleClaim : IdentityRoleClaim<Guid>
    {
        private Role role = new Role();

        private new Guid RoleId
        {
            get => role.Id;
            set => role.Id = value;
        }

        [ForeignKey("RoleId")]
        public Role Role
        {
            get => role;
            set
            {
                role = value;
                RoleId = value.Id;
            }
        }
    }
}
