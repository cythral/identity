using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Users
{
    public class User : IdentityUser<Guid>
    {
        public User()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public new Guid Id
        {
            get => base.Id;
            internal set => base.Id = value;
        }

        [NotMapped]
        [JsonIgnore]
        public string Name => UserName;

        [JsonIgnore]
        public override string NormalizedUserName { get; set; } = string.Empty;

        [JsonIgnore]
        public override string NormalizedEmail { get; set; } = string.Empty;

        [JsonIgnore]
        public override string PasswordHash { get; set; } = string.Empty;

        [JsonIgnore]
        public override string SecurityStamp { get; set; } = Guid.NewGuid().ToString();

        [JsonIgnore]
        public override string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        public virtual ICollection<UserLogin> Logins { get; set; } = new List<UserLogin>();

        public virtual ICollection<UserClaim> Claims { get; set; } = new List<UserClaim>();

        /// <summary>
        /// Gets or sets the roles this user is allowed to use.
        /// </summary>
        /// <returns>The roles this user is allowed to use.</returns>
        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}
