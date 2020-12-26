using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brighid.Identity.Users
{
    public class UserLoginAttribute
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public Guid LoginId { get; internal set; }

        [ForeignKey("LoginId")]
        public UserLogin Login { get; set; }
    }
}
