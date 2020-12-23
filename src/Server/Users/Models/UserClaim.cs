using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Users
{
    public class UserClaim : IdentityUserClaim<Guid>
    {
        [Key]
        public new Guid Id { get; set; } = Guid.NewGuid();

        public override Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
