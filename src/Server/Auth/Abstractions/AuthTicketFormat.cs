using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using OpenIddict.Server;

using static OpenIddict.Abstractions.OpenIddictConstants;

#pragma warning disable IDE0078, IDE0083

namespace Brighid.Identity.Auth
{
    public sealed class AuthTicketFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly IOptionsMonitor<OpenIddictServerOptions> openIdServerOptions;
        private readonly JwtSecurityTokenHandler tokenHandler = new();

        public AuthTicketFormat(IOptionsMonitor<OpenIddictServerOptions> openIdServerOptions)
        {
            this.openIdServerOptions = openIdServerOptions;
        }

        public AuthenticationTicket? Unprotect(string protectedText) => Unprotect(protectedText, null);

        public string Protect(AuthenticationTicket data) => Protect(data, null);

        public AuthenticationTicket? Unprotect(string protectedText, string? purpose)
        {
            try
            {
                var validationParameters = openIdServerOptions.CurrentValue.TokenValidationParameters.Clone();
                var result = openIdServerOptions.CurrentValue.JsonWebTokenHandler.ValidateToken(protectedText, validationParameters);
                if (!result.IsValid)
                {
                    throw new Exception("JWT Failed to Validate", result.Exception);
                }

                if (!(result.SecurityToken is JsonWebToken jwt))
                {
                    throw new SecurityTokenValidationException("JWT token was found to be invalid");
                }

                var identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme, Claims.Name, Claims.Role);
                identity.AddClaims(jwt.Claims);

                var principal = new ClaimsPrincipal(identity);
                var authProps = new AuthenticationProperties();

                return new AuthenticationTicket(principal, authProps, IdentityConstants.ApplicationScheme);
            }
            catch (SecurityTokenExpiredException)
            {
                return null;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
        }

        public string Protect(AuthenticationTicket data, string? purpose)
        {
            return data.Properties.GetTokenValue("access_token")!;
        }
    }
}
