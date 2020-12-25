using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity.Roles
{
    public abstract class PrincipalRoleService<TPrincipal, TPrimaryKey, TPrincipalRoleJoin, TPrincipalRoleJoinRepository> : IPrincipalRoleService<TPrincipal, TPrimaryKey, TPrincipalRoleJoin, TPrincipalRoleJoinRepository>
        where TPrincipal : IPrincipalWithRoles<TPrincipal, TPrincipalRoleJoin>
        where TPrincipalRoleJoin : class, IPrincipalRoleJoin<TPrincipal>, new()
        where TPrincipalRoleJoinRepository : IRepository<TPrincipalRoleJoin, TPrimaryKey>
    {
        private readonly TPrincipalRoleJoinRepository repository;

        private readonly IRoleRepository roleRepository;

        public PrincipalRoleService(
            TPrincipalRoleJoinRepository repository,
            IRoleRepository roleRepository
        )
        {
            this.repository = repository;
            this.roleRepository = roleRepository;
        }

        /// <inheritdoc/>
        public async Task UpdatePrincipalRoles(TPrincipal principal, ICollection<TPrincipalRoleJoin> roles)
        {
            var updatedDict = roles.ToDictionary(
                join => join.Role.Name.ToUpper(CultureInfo.InvariantCulture),
                join => join
            );

            var existingDict = principal.Roles.ToDictionary(
                join => join.Role.NormalizedName,
                join => join
            );

            foreach (var (name, role) in existingDict)
            {
                if (!updatedDict.ContainsKey(name))
                {
                    principal.Roles.Remove(role);
                    repository.TrackAsDeleted(role);
                }

                updatedDict.Remove(name);
            }

            foreach (var (name, _) in updatedDict)
            {
                await AddRoleToPrincipal(principal, name);
            }
        }

        /// <inheritdoc/>
        public async Task AddRoleToPrincipal(
            TPrincipal principal,
            string roleName,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (principal.GetRoleJoin(roleName) != null)
            {
                throw new PrincipalAlreadyHasRoleException(principal.Name, roleName);
            }

            var role = await roleRepository.FindByName(roleName, cancellationToken);
            if (role == null)
            {
                throw new RoleNotFoundException(roleName);
            }

            principal.Roles.Add(new TPrincipalRoleJoin
            {
                Principal = principal,
                Role = role,
            });
        }
    }
}
