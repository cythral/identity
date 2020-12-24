using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Roles
{
    public class Role : IdentityRole<Guid>
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
    }
}
