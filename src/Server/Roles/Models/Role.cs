using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Roles
{
    public class Role : IdentityRole<Guid>
    {
        public Role()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Gets or sets the unique identifier for this role.
        /// </summary>
        /// <value>The role's unique identifier.</value>
        [Key]
        public new Guid Id
        {
            get => base.Id;
            internal set => base.Id = value;
        }

        /// <summary>
        /// Gets or sets the normalized name for this role.
        /// </summary>
        /// <value>The roles' normalized name.</value>
        [JsonIgnore]
        public override string NormalizedName { get; set; } = "";

        /// <summary>
        /// Gets or sets the concurrency stamp for this entity.
        /// </summary>
        /// <value>The role's concurrency stamp.</value>
        [JsonIgnore]
        public override string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the description of the role.
        /// </summary>
        /// <value>The description of the role.</value>
        public string Description { get; set; } = "";

        public virtual ICollection<RoleClaim> Claims { get; set; } = new List<RoleClaim>();
    }
}
