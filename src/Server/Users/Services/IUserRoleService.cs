using System;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Users
{
    [ScopedService]
    public interface IUserRoleService : IPrincipalRoleService<User, Guid, UserRole, IUserRoleRepository>
    {
    }
}
