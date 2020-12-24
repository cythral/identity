using System;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Users
{
    public class DefaultUserRoleService : PrincipalRoleService<User, Guid, UserRole, IUserRoleRepository>, IUserRoleService
    {

        public DefaultUserRoleService(
            IUserRoleRepository userRoleRepository,
            IRoleRepository roleRepository
        ) : base(userRoleRepository, roleRepository)
        {
        }

    }
}
