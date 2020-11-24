using System.Threading;
using System.Threading.Tasks;

using AspNet.Security.OpenIdConnect.Primitives;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using Microsoft.AspNetCore.Authentication;

namespace Brighid.Identity.Auth
{
    [ScopedService]
    public interface IAuthService
    {
        Task<AuthenticationTicket> ClientExchange(OpenIdConnectRequest request, CancellationToken cancellationToken = default);
    }
}
