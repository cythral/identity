using System.Linq;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using OpenIddict.Server;
using OpenIddict.Validation;

namespace Brighid.Identity.Auth
{
    /// <inheritdoc />
    public class OptionsCertificateManager : ICertificateManager
    {
        private readonly IOptionsMonitor<OpenIddictServerOptions> serverOptions;
        private readonly IOptionsMonitor<OpenIddictValidationOptions> validationOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsCertificateManager" /> class.
        /// </summary>
        /// <param name="serverOptions">Options for the OpenIddict server.</param>
        /// <param name="validationOptions">Options for the OpenIddict validation.</param>
        public OptionsCertificateManager(
            IOptionsMonitor<OpenIddictServerOptions> serverOptions,
            IOptionsMonitor<OpenIddictValidationOptions> validationOptions
        )
        {
            this.serverOptions = serverOptions;
            this.validationOptions = validationOptions;
        }

        /// <inheritdoc />
        public void UpdateCertificates(params SigningCredentials?[] signingCredentials)
        {
            var credentialsToAdd = from signingCred in signingCredentials
                                   where signingCred != null
                                   select signingCred;

            lock (serverOptions.CurrentValue.SigningCredentials)
            {
                serverOptions.CurrentValue.SigningCredentials.Clear();
                serverOptions.CurrentValue.SigningCredentials.AddRange(credentialsToAdd);
            }

            var issuerSigningKeys = (from credential in credentialsToAdd
                                     select credential.Key).ToList();
            serverOptions.CurrentValue.TokenValidationParameters.IssuerSigningKeys = issuerSigningKeys;
            validationOptions.CurrentValue.TokenValidationParameters.IssuerSigningKeys = issuerSigningKeys;
        }
    }
}
