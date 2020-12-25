using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity.Roles
{
    public interface IPrincipalRoleService<TPrincipal, TPrimaryKey, TPrincipalRoleJoin, TPrincipalRoleJoinRepository>
        where TPrincipal : IPrincipalWithRoles<TPrincipal, TPrincipalRoleJoin>
        where TPrincipalRoleJoin : class, IPrincipalRoleJoin<TPrincipal>, new()
        where TPrincipalRoleJoinRepository : IRepository<TPrincipalRoleJoin, TPrimaryKey>
    {
        /// <summary>
        /// Updates a change-tracked principal's roles.
        /// </summary>
        /// <param name="principal">The principal to update roles for.</param>
        /// <param name="roles">The roles to set for the principal.</param>
        Task UpdatePrincipalRoles(TPrincipal principal, ICollection<TPrincipalRoleJoin> roles);

        /// <summary>
        /// Adds a role to a principal.
        /// </summary>
        /// <param name="principal">The principal to add the role to.</param>
        /// <param name="roleName">The name of the role to add the principal to.</param>
        /// <param name="cancellationToken">A token to cancel the task with.</param>
        /// <returns></returns>

        Task AddRoleToPrincipal(TPrincipal principal, string roleName, CancellationToken cancellationToken = default);
    }
}
