using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Users
{
    public class UserToken : IdentityUserToken<Guid>
    {
        [Key]
        public new string Value { get; set; }
    }
}
