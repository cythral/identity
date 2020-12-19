using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Applications
{
    [ScopedService]
    public interface IApplicationRoleRepository : IRepository<ApplicationRole, Guid>
    {
        Task<IEnumerable<Role>> FindRolesForApplication(string applicationName, CancellationToken cancellationToken = default);
    }
}
