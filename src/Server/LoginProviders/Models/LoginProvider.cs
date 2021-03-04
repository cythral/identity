using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Brighid.Identity.LoginProviders
{
    public class LoginProvider
    {
        [Key]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string UserIdField { get; set; } = string.Empty;

        public string SpaceDelimitedScopes { get; set; } = string.Empty;

        public AuthType AuthType { get; set; } = AuthType.OAuth;

        public Uri AuthorizeUrl { get; set; }

        public Uri TokenUrl { get; set; }

        public Uri UserInfoUrl { get; set; }

        public Uri? ImageUrl { get; set; }

        public IEnumerable<string> Scopes => SpaceDelimitedScopes.Split(' ');
    }
}
