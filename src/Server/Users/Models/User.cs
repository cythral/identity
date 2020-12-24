using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;

using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Users
{
    public class User : IdentityUser<Guid>, IPrincipalWithRoles<User, UserRole>
    {

        [Key]
        public override Guid Id { get; set; } = Guid.NewGuid();

        [NotMapped]
        [JsonIgnore]
        public string Name => UserName;

        [JsonIgnore]
        public override string NormalizedUserName { get; set; } = "";

        [JsonIgnore]
        public override string NormalizedEmail { get; set; } = "";

        [JsonIgnore]
        public override string PasswordHash { get; set; } = "";

        [JsonIgnore]
        public override string SecurityStamp { get; set; } = Guid.NewGuid().ToString();

        [JsonIgnore]
        public override string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        public virtual ICollection<UserLogin> Logins { get; set; } = new List<UserLogin>();

        public virtual ICollection<UserClaim> Claims { get; set; } = new List<UserClaim>();

        /// <summary>
        /// Gets the roles this user is allowed to use.
        /// </summary>
        /// <returns>The roles this user is allowed to use.</returns>
        [InverseProperty("User")]
        public virtual ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    }
}
