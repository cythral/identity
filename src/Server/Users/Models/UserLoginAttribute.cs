using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brighid.Identity.Users
{
    public class UserLoginAttribute
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public Guid LoginId { get; set; }

        [ForeignKey("LoginId")]
        public UserLogin Login { get; set; }
    }
}
