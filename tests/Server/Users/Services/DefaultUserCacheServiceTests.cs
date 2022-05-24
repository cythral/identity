using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Amazon.SimpleNotificationService;

using AutoFixture.NUnit3;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.Users
{
    public class DefaultUserCacheServiceTests
    {
        [TestFixture]
        [Category("Unit")]
        public class ClearExternalUserCacheTests
        {
            [Test]
            [Auto]
            public async Task ShouldNotifySns(
                Guid userId,
                [Frozen] AppConfig config,
                [Frozen] IAmazonSimpleNotificationService sns,
                [Target] DefaultUserCacheService service,
                CancellationToken cancellationToken
            )
            {
                await service.ClearExternalUserCache(userId, cancellationToken);

                await sns.Received().PublishAsync(
                    Is(config.CacheTopic),
                    Is<string>(message => JsonSerializer.Deserialize<CacheExpirationRequest>(message, null as JsonSerializerOptions).Id == userId.ToString()),
                    Is(cancellationToken)
                );
            }
        }
    }
}
