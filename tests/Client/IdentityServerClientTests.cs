using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;

using AutoFixture.NUnit3;

using Brighid.Identity.Applications;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using OpenIddict.Server;
using RichardSzalay.MockHttp;
using RichardSzalay.MockHttp.Matchers;

using static NSubstitute.Arg;
using static System.Text.Json.JsonSerializer;

namespace Brighid.Identity.Client
{
    public class IdentityServerClientTests
    {
        [Test, Auto]
        public async Task ExchangeClientCredentialsForToken_ShouldSendRequestWithClientId(
            Uri baseAddress,
            string clientId,
            string clientSecret,
            Token response
        )
        {
            using var handler = new MockHttpMessageHandler();
            using var httpClient = new HttpClient(handler) { BaseAddress = baseAddress };
            var client = new IdentityServerClient(httpClient);

            handler
            .Expect($"{baseAddress}oauth2/token")
            .WithFormData("client_id", clientId)
            .Respond("application/json", Serialize(response));

            await client.ExchangeClientCredentialsForToken(clientId, clientSecret);

            handler.VerifyNoOutstandingExpectation();
        }

        [Test, Auto]
        public async Task ExchangeClientCredentialsForToken_ShouldSendRequestWithClientSecret(
            Uri baseAddress,
            string clientId,
            string clientSecret,
            Token response
        )
        {
            using var handler = new MockHttpMessageHandler();
            using var httpClient = new HttpClient(handler) { BaseAddress = baseAddress };
            var client = new IdentityServerClient(httpClient);

            handler
            .Expect($"{baseAddress}oauth2/token")
            .WithFormData("client_secret", clientSecret)
            .Respond("application/json", Serialize(response));

            await client.ExchangeClientCredentialsForToken(clientId, clientSecret);

            handler.VerifyNoOutstandingExpectation();
        }

        [Test, Auto]
        public async Task ExchangeClientCredentialsForToken_ShouldSendRequestWithGrantTypeClientCredentials(
            Uri baseAddress,
            string clientId,
            string clientSecret,
            Token response
        )
        {
            using var handler = new MockHttpMessageHandler();
            using var httpClient = new HttpClient(handler) { BaseAddress = baseAddress };
            var client = new IdentityServerClient(httpClient);

            handler
            .Expect($"{baseAddress}oauth2/token")
            .WithFormData("grant_type", "client_credentials")
            .Respond("application/json", Serialize(response));

            await client.ExchangeClientCredentialsForToken(clientId, clientSecret);

            handler.VerifyNoOutstandingExpectation();
        }

        [Test, Auto]
        public async Task ExchangeClientCredentialsForToken_ShouldReturnToken(
            Uri baseAddress,
            string clientId,
            string clientSecret,
            Token token
        )
        {
            using var handler = new MockHttpMessageHandler();
            using var httpClient = new HttpClient(handler) { BaseAddress = baseAddress };
            var client = new IdentityServerClient(httpClient);

            handler
            .Expect($"{baseAddress}oauth2/token")
            .Respond("application/json", Serialize(token));

            var response = await client.ExchangeClientCredentialsForToken(clientId, clientSecret);

            response.Should().BeEquivalentTo(token, options =>
                options.Excluding(t => t.CreationDate)
            );

            handler.VerifyNoOutstandingExpectation();
        }

        [Test, Auto]
        public async Task ExchangeClientCredentialsForToken_ShouldThrowIfTokenIsNull(
            Uri baseAddress,
            string clientId,
            string clientSecret
        )
        {
            using var handler = new MockHttpMessageHandler();
            using var httpClient = new HttpClient(handler) { BaseAddress = baseAddress };
            var client = new IdentityServerClient(httpClient);

            handler
            .Expect($"{baseAddress}oauth2/token")
            .Respond("application/json", "null");

            Func<Task<Token>> func = async () => await client.ExchangeClientCredentialsForToken(clientId, clientSecret);
            await func.Should().ThrowAsync<Exception>();

            handler.VerifyNoOutstandingExpectation();
        }
    }
}
