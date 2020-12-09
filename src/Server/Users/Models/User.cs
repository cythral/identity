using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Users
{
    public class User : IdentityUser
    {
        public ICollection<UserLogin> Logins { get; set; }
    }
}
