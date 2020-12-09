using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brighid.Identity.Users
{
    [Table("UserLogins")]
    public class UserLogin : IdentityUserLogin<string>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        public ICollection<UserLoginAttribute> Attributes { get; set; }
    }
}
