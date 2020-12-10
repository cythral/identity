using System;
using System.Security.Claims;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Users
{
    public class User : IdentityUser<Guid>
    {
        [Key]
        public new Guid Id { get; set; } = Guid.NewGuid();

        public ICollection<UserLogin> Logins { get; set; }

        public ICollection<UserClaim> Claims { get; set; }

        /// <summary>
        /// Gets a collection of application roles that belong to this User. This is the backing property for roles.
        /// </summary>
        /// <value>The role mappings this user is allowed to use.</value>
        private ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        /// <summary>
        /// Gets the roles this application is allowed to use.
        /// </summary>
        /// <returns>The roles this application is allowed to use.</returns>
        [NotMapped]
        public IEnumerable<string> Roles
        {
            get => UserRoles.Select(userRole => userRole.Role.Name);
            set => UserRoles = value
                    .Select(role => new UserRole(this, role))
                    .ToList();
        }
    }
}
