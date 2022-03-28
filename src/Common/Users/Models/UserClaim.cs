using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Users
{
    public class UserClaim : IdentityUserClaim<Guid>
    {
        [Key]
        public new Guid Id { get; internal set; } = Guid.NewGuid();

        [JsonIgnore]
        public override Guid UserId { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual User User { get; set; }
    }
}
