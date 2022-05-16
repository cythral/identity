using System;
using System.Security.Claims;

namespace Brighid.Identity.Users
{
    /// <summary>
    /// Service for working with Principals.
    /// </summary>
    public interface IPrincipalService
    {
        /// <summary>
        /// Gets the user id from the given principal.
        /// </summary>
        /// <param name="principal">The principal to get their id for.</param>
        /// <returns>The principal's ID.</returns>
        Guid GetId(ClaimsPrincipal principal);
    }
}
