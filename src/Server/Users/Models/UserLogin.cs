using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brighid.Identity.Users
{
    public class UserLogin : IdentityUserLogin<Guid>
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public ICollection<UserLoginAttribute> Attributes { get; set; }
    }
}
