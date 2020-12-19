using System;
using System.Globalization;
using System.Security.Claims;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

using Brighid.Identity.Roles;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Users
{
    public class User : IdentityUser<Guid>
    {

        [Key]
        public override Guid Id { get; set; } = Guid.NewGuid();

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
