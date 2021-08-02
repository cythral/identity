using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MySqlConnector;

#pragma warning disable IDE0046

namespace Brighid.Identity.Roles
{
    public class DefaultRoleService : IRoleService
    {
        private readonly IRoleRepository repository;

        public DefaultRoleService(
            IRoleRepository repository
        )
        {
            this.repository = repository;
        }

        /// <inheritdoc />
        public void ValidateRoleDelegations(IEnumerable<string> roles, ClaimsPrincipal principal)
        {
            foreach (var role in roles)
            {
                var attributes = typeof(BuiltInRole).GetField(role)?.GetCustomAttributes<DelegatingRoleAttribute>();

                if (attributes == null || !attributes.Any())
                {
                    return;
                }

                foreach (var attribute in attributes)
                {
                    if (principal.IsInRole(attribute.Role))
                    {
                        return;
                    }
                }

                throw new RoleDelegationDeniedException($"{principal.Identity?.Name} is not allowed to delegate {role}.");
            }
        }

        /// <inheritdoc />
        public void ValidateUserHasRoles(IEnumerable<string> roles, ClaimsPrincipal principal)
        {
            foreach (var role in roles)
            {
                if (!principal.IsInRole(role))
                {
                    throw new RoleRequiredException(role);
                }
            }
        }

        /// <inheritdoc />
        public async Task<Role> Create(RoleRequest role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                role.NormalizedName = role.Name.ToUpper(CultureInfo.InvariantCulture);
                var result = await repository.Add(role);
                return result;
            }
            catch (DbUpdateException e)
                when ((e.InnerException as MySqlException)?.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
            {
                throw new EntityAlreadyExistsException($"A Role already exists with the name {role.Name}");
            }
        }

        /// <inheritdoc />
        public async Task<Role> UpdateById(Guid id, RoleRequest updatedRoleInfo, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var existingRole = await repository.FindById(id);
            if (existingRole == null)
            {
                throw new EntityNotFoundException($"A Role with ID {id} was not found.");
            }

            if (existingRole.Name != updatedRoleInfo.Name)
            {
                throw new NotSupportedException("Updating role names is not supported.");
            }

            existingRole.Description = updatedRoleInfo.Description;
            return await repository.Save(existingRole);
        }

        /// <inheritdoc />
        public async Task<Role> DeleteById(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await repository.IsAttachedToAPrincipal(id, cancellationToken))
            {
                throw new NotSupportedException("Cannot delete a role that is attached to either an application or user.");
            }

            return await repository.Remove(id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Role>> List(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await repository.List();
        }

        /// <inheritdoc />
        public async Task<Role?> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await repository.FindById(id);
        }

        /// <inheritdoc />
        public async Task<Role?> GetByName(string name, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await repository.FindByName(name, cancellationToken);
        }
    }
}
