using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

using Microsoft.Extensions.Logging;

namespace Brighid.Identity.Auth
{
    /// <inheritdoc />
    public class SsmLinkStartUrlService : ILinkStartUrlService
    {
        private readonly IAmazonSimpleSystemsManagement ssmClient;
        private readonly ILogger<SsmLinkStartUrlService> logger;
        private readonly ConcurrentDictionary<string, string> cache = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SsmLinkStartUrlService" /> class.
        /// </summary>
        /// <param name="ssmClient">Client used to make SSM requests.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public SsmLinkStartUrlService(
            IAmazonSimpleSystemsManagement ssmClient,
            ILogger<SsmLinkStartUrlService> logger
        )
        {
            this.ssmClient = ssmClient;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task<string?> GetLinkStartUrlForProvider(string providerName, CancellationToken cancellationToken = default)
        {
            if (cache.TryGetValue(providerName, out var cachedUrl))
            {
                return cachedUrl;
            }

            if (!Regex.Match(providerName, @"^[a-zA-Z-]+$").Success)
            {
                return null;
            }

            try
            {
                var request = new GetParameterRequest { Name = $"/brighid/{providerName}/account-link/start-url" };
                logger.LogInformation("Sending ssm:GetParameter with {@request}", request);

                var response = await ssmClient.GetParameterAsync(request, cancellationToken);
                logger.LogInformation("Received ssm:GetParameter response: {@response}", response);

                var value = response.Parameter.Value;
                return cache.GetOrAdd(providerName, response.Parameter.Value);
            }
            catch (Exception exception)
            {
                logger.LogError("Received error from ssm:GetParameter request: {@exception}", exception);
                return null;
            }
        }
    }
}
