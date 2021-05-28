using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using static OpenIddict.Abstractions.OpenIddictConstants;

#pragma warning disable IDE0078, IDE0083

namespace Brighid.Identity.Auth
{
    public sealed class AuthTicketFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly TokenValidationParameters validationParameters;
        private readonly JwtSecurityTokenHandler tokenHandler = new();

        public AuthTicketFormat(TokenValidationParameters validationParameters)
        {
            this.validationParameters = validationParameters;
        }

        public AuthenticationTicket? Unprotect(string protectedText) => Unprotect(protectedText, null);
        public string Protect(AuthenticationTicket data) => Protect(data, null);

        public AuthenticationTicket? Unprotect(string protectedText, string? purpose)
        {
            try
            {
                tokenHandler.ValidateToken(protectedText, validationParameters, out var token);

                if (!(token is JwtSecurityToken jwt))
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
            catch (Exception)
            {
                return null;
            }
        }

        public string Protect(AuthenticationTicket data, string? purpose)
        {
            return data.Properties.GetTokenValue("jwt")!;
        }
    }
}
