using System;
using System.Collections.Generic;
using System.Security.Claims;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Roles
{
    [ScopedService]
    public interface IRoleService : IEntityService<Role, Guid>
    {
        /// <summary>
        /// Validates that the given <paramref name="principal" /> is allowed to delegate
        /// all of the given <paramref name="roles" /> to another principal.
        /// </summary>
        /// <param name="roles">The roles to validate delegations for.</param>
        /// <param name="principal">The principal to validate delegations for.</param>
        void ValidateRoleDelegations(IEnumerable<string> roles, ClaimsPrincipal principal);

        /// <summary>
        /// Validates that the given <paramref name="principal" /> has all of
        /// the given <paramref name="roles" />.
        /// </summary>
        /// <param name="roles">The roles that the principal should have.</param>
        /// <param name="principal">The principal to check roles for.</param>
        void ValidateUserHasRoles(IEnumerable<string> roles, ClaimsPrincipal principal);
    }
}
