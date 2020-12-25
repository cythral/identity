using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using RichardSzalay.MockHttp;

using static NSubstitute.Arg;

namespace Brighid.Identity.Client
{
#pragma warning disable IDE0055
    [Category("Unit")]
    public class ClientCredentialsHandlerTests
    {
        [Test, Auto]
        public async Task SendAsync_ShouldRetrieveTokenAndStoreIt_IfItsNotInTheCache(
            Token token,
            HttpRequestMessage requestMessage,
            [NotNull, Substitute, Frozen] TokenCache tokenCache,
            [NotNull, Substitute, Frozen] IClientCredentials clientCredentials,
            [NotNull, Substitute, Frozen] IdentityServerClient identityServerClient,
            [NotNull, Target] ClientCredentialsHandler<IClientCredentials> handler
        )
        {
            var cancellationToken = new CancellationToken(false);
            using var mockHttp = new MockHttpMessageHandler();
            handler.InnerHandler = mockHttp;

            using var invoker = new HttpMessageInvoker(handler);
            tokenCache.Token.Returns((Token?)null);
            identityServerClient
            .ExchangeClientCredentialsForToken(Any<string>(), Any<string>(), Any<CancellationToken>())
            .Returns(token);

            await invoker.SendAsync(requestMessage, cancellationToken);

            tokenCache.Token.Should().Be(token);
            await identityServerClient.Received().ExchangeClientCredentialsForToken(
                Is(clientCredentials.ClientId),
                Is(clientCredentials.ClientSecret),
                Is(cancellationToken)
            );
        }

        [Test, Auto]
        public async Task SendAsync_ShouldNotRetrieveToken_IfItsInTheCache(
            string response,
            Uri uri,
            Token token,
            HttpRequestMessage requestMessage,
            [Substitute, Frozen] TokenCache tokenCache,
            [Substitute, Frozen] IClientCredentials clientCredentials,
            [Substitute, Frozen] IdentityServerClient identityServerClient,
            [NotNull, Target] ClientCredentialsHandler<IClientCredentials> handler
        )
        {
            var cancellationToken = new CancellationToken(false);
            using var mockHttp = new MockHttpMessageHandler();
            handler.InnerHandler = mockHttp;
            mockHttp
            .Expect(uri.ToString())
            .Respond("text/plain", response);


            using var invoker = new HttpMessageInvoker(handler);
            tokenCache.Token.Returns(token);

            await invoker.SendAsync(requestMessage, cancellationToken);

            await identityServerClient.DidNotReceive().ExchangeClientCredentialsForToken(
                Is(clientCredentials.ClientId),
                Is(clientCredentials.ClientSecret),
                Is(cancellationToken)
            );
        }

        [Test, Auto]
        public async Task SendAsync_AttachesIdentityToken_ToRequests(
            string response,
            Uri uri,
            Token token,
            HttpRequestMessage requestMessage,
            [NotNull, Substitute, Frozen] TokenCache tokenCache,
            [NotNull, Target] ClientCredentialsHandler<IClientCredentials> handler
        )
        {
            var cancellationToken = new CancellationToken(false);
            using var mockHttp = new MockHttpMessageHandler();
            handler.InnerHandler = mockHttp;
            mockHttp
            .Expect(uri.ToString())
            .WithHeaders("Authorization", $"Bearer {token.IdToken}")
            .Respond("text/plain", response);

            using var invoker = new HttpMessageInvoker(handler);
            tokenCache.Token.Returns(token);

            requestMessage.RequestUri = uri;
            await invoker.SendAsync(requestMessage, cancellationToken);

            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}
