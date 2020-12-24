using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Users
{
    [ScopedService]
    public interface IUserRoleRepository : IRepository<UserRole, Guid>
    {
        Task<IEnumerable<Role>> FindRolesForUser(Guid userId, CancellationToken cancellationToken = default);
    }
}
