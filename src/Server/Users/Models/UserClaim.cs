using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Brighid.Identity.Users
{
    public class UserClaim : IdentityUserClaim<Guid>
    {
        private User user = new User();

        private new Guid UserId
        {
            get => user.Id;
            set => user.Id = value;
        }

        [ForeignKey("UserId")]
        public User User
        {
            get => user;
            set
            {
                user = value;
                UserId = value.Id;
            }
        }
    }
}
