using System;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using RichardSzalay.MockHttp;

using static System.Text.Json.JsonSerializer;

namespace Brighid.Identity.Client
{
    [Category("Unit")]
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
