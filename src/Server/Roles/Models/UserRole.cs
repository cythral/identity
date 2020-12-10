using System;
using System.ComponentModel.DataAnnotations.Schema;

using Brighid.Identity.Users;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Roles
{
    public class UserRole : IdentityUserRole<Guid>
    {

        private User user = new User();

        private Role role = new Role();

        public UserRole(User user, string roleName)
        {
            User = user;
            UserId = user.Id;
            Role = new Role { Name = roleName };
        }

        public UserRole()
        {
        }

        private new Guid UserId
        {
            get => user.Id;
            set => user.Id = value;
        }

        private new Guid RoleId
        {
            get => role.Id;
            set => role.Id = value;
        }

        [ForeignKey("UserId")]
        public User User
        {
            get => user;
            set
            {
                user = value;
                UserId = value.Id;
            }
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
