using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Brighid.Identity.Interface.Auth
{
    public class HttpCookieHandler : DelegatingHandler
    {
        public HttpCookieHandler()
        {
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            message.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            return await base.SendAsync(message, cancellationToken);
        }
    }
}
