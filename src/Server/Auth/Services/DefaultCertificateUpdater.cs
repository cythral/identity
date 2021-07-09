using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Brighid.Identity.Auth
{
    /// <inheritdoc />
    public class DefaultCertificateUpdater : ICertificateUpdater
    {
        private readonly ICertificateConfigurationService certificateConfigService;
        private readonly ICertificateFetcher certificateFetcher;
        private readonly ICertificateManager certificateManager;
        private readonly ILogger<DefaultCertificateUpdater> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCertificateUpdater" /> class.
        /// </summary>
        /// <param name="certificateConfigService">Service to fetch certificate configuration.</param>
        /// <param name="certificateFetcher">Service to fetch certificates with.</param>
        /// <param name="certificateManager">Service to manage certificates with.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public DefaultCertificateUpdater(
            ICertificateConfigurationService certificateConfigService,
            ICertificateFetcher certificateFetcher,
            ICertificateManager certificateManager,
            ILogger<DefaultCertificateUpdater> logger
        )
        {
            this.certificateConfigService = certificateConfigService;
            this.certificateFetcher = certificateFetcher;
            this.certificateManager = certificateManager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task UpdateCertificates(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Performing certificate updates.");
            var configuration = await certificateConfigService.GetConfiguration(cancellationToken);
            var activeCertificate = await certificateFetcher.FetchCertificate(configuration.BucketName, configuration.ActiveCertificateHash, cancellationToken);
            var inactiveCertificate = await GetCertificateIfHashNotNull(configuration.BucketName, configuration.InactiveCertificateHash, cancellationToken);

            if (activeCertificate == null)
            {
                logger.LogCritical("An active signing certificate was not found in the certificate configuration. Please ensure the identity-certificates service has run before starting the core identity service.");
                throw new Exception("An active signing certificate is required.");
            }

            certificateManager.UpdateCertificates(activeCertificate, inactiveCertificate);
            logger.LogInformation("Successfully updated certificates.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<SigningCredentials?> GetCertificateIfHashNotNull(string bucket, string? hash, CancellationToken cancellationToken)
        {
            return hash == null
                ? null
                : await certificateFetcher.FetchCertificate(bucket, hash, cancellationToken);
        }
    }
}
