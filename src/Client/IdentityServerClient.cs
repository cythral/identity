using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity.Client
{
    public class IdentityServerClient
    {
        private static readonly Uri tokenUri = new("/oauth2/token", UriKind.Relative);
        private readonly HttpClient httpClient;

        public IdentityServerClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public virtual async Task<Token> ExchangeClientCredentialsForToken(string clientId, string clientSecret, CancellationToken cancellationToken = default)
        {
            var formData = (IEnumerable<KeyValuePair<string?, string?>>)new Dictionary<string, string?>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["grant_type"] = "client_credentials",
            };

            using var requestContent = new FormUrlEncodedContent(formData);
            var response = await httpClient.PostAsync(tokenUri, requestContent, cancellationToken);
            var token = await response.Content.ReadFromJsonAsync<Token>(cancellationToken: cancellationToken);

            return token switch
            {
                Token => token,
                _ => throw new Exception("Token unexpectedly deserialized to null value."),
            };
        }
    }
}
