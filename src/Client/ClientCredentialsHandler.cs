using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace Brighid.Identity.Client
{
    public class ClientCredentialsHandler : DelegatingHandler
    {
        private readonly TokenCache tokenCache;
        private readonly IdentityServerClient identityServerClient;
        private readonly BrighidClientCredentials credentials;

        public ClientCredentialsHandler(TokenCache tokenCache, IdentityServerClient identityServerClient, IOptions<BrighidClientCredentials> options)
        {
            this.tokenCache = tokenCache;
            this.identityServerClient = identityServerClient;
            this.credentials = options.Value;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
        {
            tokenCache.Token ??= await identityServerClient
                .ExchangeClientCredentialsForToken(credentials.ClientId, credentials.ClientSecret, cancellationToken)
                .ConfigureAwait(false);

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenCache.Token.IdToken);
            return await base.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
        }
    }
}
