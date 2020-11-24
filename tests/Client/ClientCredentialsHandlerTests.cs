using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Applications;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using OpenIddict.Server;

using RichardSzalay.MockHttp;
using RichardSzalay.MockHttp.Matchers;

using static NSubstitute.Arg;

namespace Brighid.Identity.Client
{
#pragma warning disable IDE0055
    public class ClientCredentialsHandlerTests
    {
        [Test, Auto]
        public async Task SendAsync_ShouldRetrieveTokenAndStoreIt_IfItsNotInTheCache(
            Token token,
            HttpRequestMessage requestMessage,
            [NotNull, Substitute, Frozen] TokenCache tokenCache,
            [NotNull, Substitute, Frozen] BrighidClientCredentials clientCredentials,
            [NotNull, Substitute, Frozen] IdentityServerClient identityServerClient,
            [NotNull, Target] ClientCredentialsHandler handler
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
            [Substitute, Frozen] BrighidClientCredentials clientCredentials,
            [Substitute, Frozen] IdentityServerClient identityServerClient,
            [NotNull, Target] ClientCredentialsHandler handler
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
            [NotNull, Target] ClientCredentialsHandler handler
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