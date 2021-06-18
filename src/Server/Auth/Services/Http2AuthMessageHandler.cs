using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity.Auth
{
    public class Http2AuthMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            request.Version = new Version(2, 0);
            request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
