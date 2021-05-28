using System;
using System.Threading;
using System.Threading.Tasks;

using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.Auth
{
    public class SsmLinkStartUrlServiceTests
    {
        [TestFixture]
        public class GetStartLinkForProviderTests
        {
            [Test]
            [Auto]
            public async Task ShouldFetchTheStartLinkFromSsm(
                string url,
                [Frozen] GetParameterResponse response,
                [Frozen, Substitute] IAmazonSimpleSystemsManagement ssmClient,
                [Target] SsmLinkStartUrlService service,
                CancellationToken cancellationToken
            )
            {
                response.Parameter.Value = url;
                var provider = "discord";

                var result = await service.GetLinkStartUrlForProvider(provider, cancellationToken);

                result.Should().Be(url);
                await ssmClient.Received().GetParameterAsync(Is<GetParameterRequest>(req => req.Name == $"/brighid/{provider}/account-link/start-url"), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldReturnNullIfParameterDoesntExist(
                string url,
                [Frozen] GetParameterResponse response,
                [Frozen, Substitute] IAmazonSimpleSystemsManagement ssmClient,
                [Target] SsmLinkStartUrlService service,
                CancellationToken cancellationToken
            )
            {
                response.Parameter.Value = url;
                var provider = "discord";

                ssmClient.GetParameterAsync(Any<GetParameterRequest>(), Any<CancellationToken>()).Throws<Exception>();

                var result = await service.GetLinkStartUrlForProvider(provider, cancellationToken);

                result.Should().Be(null);
                await ssmClient.Received().GetParameterAsync(Is<GetParameterRequest>(req => req.Name == $"/brighid/{provider}/account-link/start-url"), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldCacheResponses(
                string url,
                [Frozen] GetParameterResponse response,
                [Frozen, Substitute] IAmazonSimpleSystemsManagement ssmClient,
                [Target] SsmLinkStartUrlService service,
                CancellationToken cancellationToken
            )
            {
                response.Parameter.Value = url;

                var provider = "discord";
                await service.GetLinkStartUrlForProvider(provider, cancellationToken);
                await service.GetLinkStartUrlForProvider(provider, cancellationToken);

                await ssmClient.Received(1).GetParameterAsync(Is<GetParameterRequest>(req => req.Name == $"/brighid/{provider}/account-link/start-url"), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldNotCallSsmAndReturnNullIfProviderContainsNumbers(
                string url,
                [Frozen] GetParameterResponse response,
                [Frozen, Substitute] IAmazonSimpleSystemsManagement ssmClient,
                [Target] SsmLinkStartUrlService service,
                CancellationToken cancellationToken
            )
            {
                response.Parameter.Value = url;

                var provider = "1";
                var result = await service.GetLinkStartUrlForProvider(provider, cancellationToken);

                result.Should().BeNull();
                await ssmClient.DidNotReceive().GetParameterAsync(Is<GetParameterRequest>(req => req.Name == $"/brighid/{provider}/account-link/start-url"), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldAllowHyphens(
                string url,
                [Frozen] GetParameterResponse response,
                [Frozen, Substitute] IAmazonSimpleSystemsManagement ssmClient,
                [Target] SsmLinkStartUrlService service,
                CancellationToken cancellationToken
            )
            {
                response.Parameter.Value = url;

                var provider = "text-message";
                var result = await service.GetLinkStartUrlForProvider(provider, cancellationToken);

                result.Should().Be(url);
                await ssmClient.Received().GetParameterAsync(Is<GetParameterRequest>(req => req.Name == $"/brighid/{provider}/account-link/start-url"), Is(cancellationToken));
            }
        }
    }
}
