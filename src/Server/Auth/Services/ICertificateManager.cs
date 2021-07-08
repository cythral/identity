using Microsoft.IdentityModel.Tokens;

namespace Brighid.Identity.Auth
{
    /// <summary>
    /// Manages the signing certificates that the server and validation components use.
    /// </summary>
    public interface ICertificateManager
    {
        /// <summary>
        /// Update the signing credentials that the OpenIddict server uses.
        /// </summary>
        /// <param name="signingCredentials">The signing credentials to use in the server.</param>
        void UpdateCertificates(params SigningCredentials?[] signingCredentials);
    }
}
