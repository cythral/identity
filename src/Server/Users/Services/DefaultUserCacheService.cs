using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Amazon.SimpleNotificationService;

using Microsoft.Extensions.Options;

namespace Brighid.Identity.Users
{
    /// <inheritdoc />
    public class DefaultUserCacheService : IUserCacheService
    {
        private readonly AppConfig config;
        private readonly IAmazonSimpleNotificationService sns;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultUserCacheService" /> class.
        /// </summary>
        /// <param name="config">App-wide configuration.</param>
        /// <param name="sns">Amazon SNS Client.</param>
        public DefaultUserCacheService(
            IOptions<AppConfig> config,
            IAmazonSimpleNotificationService sns
        )
        {
            this.config = config.Value;
            this.sns = sns;
        }

        /// <inheritdoc />
        public async Task ClearExternalUserCache(Guid userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var message = JsonSerializer.Serialize(new CacheExpirationRequest(userId.ToString(), CacheExpirationRequestType.User));
            await sns.PublishAsync(config.CacheTopic, message, cancellationToken);
        }
    }
}
