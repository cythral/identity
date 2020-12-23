using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

namespace Brighid.Identity.Auth
{
    public class LoginRequest
    {
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [BindProperty]
        public Uri RedirectUri { get; set; } = new Uri("/", UriKind.Relative);
    }
}
