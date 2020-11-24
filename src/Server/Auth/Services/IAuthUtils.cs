using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using Microsoft.AspNetCore.Authentication;

namespace Brighid.Identity.Auth
{
    [ScopedService]
    public interface IAuthUtils
    {
        Task<ClaimsIdentity> CreateClaimsIdentity(string applicationName, CancellationToken cancellationToken = default);
        AuthenticationTicket CreateAuthTicket(ClaimsIdentity claimsIdentity, IEnumerable<string>? scopes = null);
    }
}
