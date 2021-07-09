using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Brighid.Identity.Auth
{
    /// <inheritdoc />
    public class DefaultCertificateConfigurationService : ICertificateConfigurationService
    {
        private readonly IAmazonSimpleSystemsManagement ssmClient;
        private readonly AuthConfig options;
        private readonly ILogger<DefaultCertificateConfigurationService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCertificateConfigurationService" /> class.
        /// </summary>
        /// <param name="ssmClient">Client for AWS SSM.</param>
        /// <param name="options">Options to use when dealing with certificates.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public DefaultCertificateConfigurationService(
            IAmazonSimpleSystemsManagement ssmClient,
            IOptions<AuthConfig> options,
            ILogger<DefaultCertificateConfigurationService> logger
        )
        {
            this.ssmClient = ssmClient;
            this.options = options.Value;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task<Configuration> GetConfiguration(CancellationToken cancellationToken = default)
        {
            var request = new GetParameterRequest { Name = options.CertificateConfigurationParameterName };
            logger.LogInformation("Sending ssm:GetParameter request: {@request}", request);

            var response = await ssmClient.GetParameterAsync(request, cancellationToken);
            logger.LogInformation("Received ssm:GetParameter response: {@response}", response);

            var configuration = JsonSerializer.Deserialize<Configuration>(response.Parameter.Value);
            return configuration ?? throw new Exception("Could not load certificate configuration from SSM.");
        }
    }
}
