using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Users
{
    public class UserToken : IdentityUserToken<Guid>
    {
        [Key]
        public new string Value { get; set; }
    }
}
