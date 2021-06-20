using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity.Roles
{
    /// <summary>
    /// Service used to interact with roles.
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Get a list of all roles.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The list of roles.</returns>
        Task<IEnumerable<Role>> List(CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new role.
        /// </summary>
        /// <param name="request">The role to create.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting role.</returns>
        Task<Role> Create(RoleRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a role by its ID.
        /// </summary>
        /// <param name="id">The ID of the role to retrieve.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The requested role, or null if not found.</returns>
        Task<Role?> GetById(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a role by its name.
        /// </summary>
        /// <param name="name">The name of the role to retrieve.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The requested role, or null if not found.</returns>
        Task<Role?> GetByName(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a role by its ID.
        /// </summary>
        /// <param name="id">The ID of the role to update.</param>
        /// <param name="request">The data to update the role with.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting role.</returns>
        Task<Role> UpdateById(Guid id, RoleRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a role by its ID.
        /// </summary>
        /// <param name="id">The ID of the role to delete.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The deleted role.</returns>
        Task<Role> DeleteById(Guid id, CancellationToken cancellationToken = default);

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
