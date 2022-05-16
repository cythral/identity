using System;
using System.Security.Claims;

using OpenIddict.Abstractions;

namespace Brighid.Identity.Users
{
    /// <inheritdoc />
    public class DefaultPrincipalService : IPrincipalService
    {
        /// <inheritdoc />
        public Guid GetId(ClaimsPrincipal principal)
        {
            var sub = principal.GetClaim("sub") ?? throw new InvalidPrincipalException("The given principal did not have a sub claim.");
            return Guid.TryParse(sub, out var id)
                ? id
                : throw new InvalidPrincipalException("The given principal had an invalid ID.");
        }
    }
}
