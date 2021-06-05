using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
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

        public Guid GetPrimaryKey(Role role) => role.Id;

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

                throw new RoleDelegationDeniedException();
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
        public async Task<Role> Create(Role role)
        {
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
        public async Task<Role> UpdateById(Guid id, Role updatedRoleInfo)
        {
            var existingRole = await repository.FindById(id);
            if (existingRole == null)
            {
                throw new EntityNotFoundException($"A Role with ID {id} was not found.");
            }

            return await UpdateCore(existingRole, updatedRoleInfo);
        }

        /// <inheritdoc />
        public async Task<Role> DeleteById(Guid id)
        {
            if (await repository.IsAttachedToAPrincipal(id))
            {
                throw new NotSupportedException("Cannot delete a role that is attached to either an application or user.");
            }

            return await repository.Remove(id);
        }

        private async Task<Role> UpdateCore(Role existingRole, Role updatedRoleInfo)
        {
            if (existingRole.Name != updatedRoleInfo.Name)
            {
                throw new NotSupportedException("Updating role names is not supported.");
            }

            existingRole.Description = updatedRoleInfo.Description;
            return await repository.Save(existingRole);
        }
    }
}
