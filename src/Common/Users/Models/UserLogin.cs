using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Identity;

#pragma warning disable IDE0025

namespace Brighid.Identity.Users
{
    public class UserLogin : IdentityUserLogin<Guid>
    {
        [Key]
        public Guid Id { get; internal set; } = Guid.NewGuid();

        [JsonIgnore]
        public override Guid UserId { get; set; }

        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User User { get; set; }

        public ICollection<UserLoginAttribute> Attributes { get; set; } = new List<UserLoginAttribute>();
    }
}
