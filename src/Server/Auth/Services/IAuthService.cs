using System.Threading;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using Microsoft.AspNetCore.Authentication;

using OpenIddict.Abstractions;

namespace Brighid.Identity.Auth
{
    [ScopedService]
    public interface IAuthService
    {
        Task<AuthenticationTicket> ClientExchange(OpenIddictRequest request, CancellationToken cancellationToken = default);
    }
}
