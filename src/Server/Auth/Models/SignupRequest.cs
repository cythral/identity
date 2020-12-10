using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

namespace Brighid.Identity.Auth
{
    public class SignupRequest
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        public Uri RedirectUri { get; set; } = new Uri("/", UriKind.Relative);
    }
}
