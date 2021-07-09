using System.Collections.Generic;
using System.Linq;

using Microsoft.IdentityModel.Tokens;

namespace Brighid.Identity.Auth
{
    public class StartupCertificateManager : ICertificateManager
    {
        private readonly List<SigningCredentials> startupCertificates;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupCertificateManager" /> class.
        /// </summary>
        /// <param name="startupCertificates">The model to store startup certificates in.</param>
        public StartupCertificateManager(
            List<SigningCredentials> startupCertificates
        )
        {
            this.startupCertificates = startupCertificates;
        }

        /// <inheritdoc />
        public void UpdateCertificates(params SigningCredentials?[] signingCredentials)
        {
            var credentialsToAdd = from credential in signingCredentials
                                   where credential != null
                                   select credential;

            startupCertificates.Clear();
            startupCertificates.AddRange(credentialsToAdd);
        }
    }
}
