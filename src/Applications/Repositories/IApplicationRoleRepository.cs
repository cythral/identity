using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Applications
{
    [ScopedService]
    public interface IApplicationRoleRepository : IRepository<ApplicationRole, ulong>
    {
        Task<IEnumerable<Role>> FindRolesForApplication(string applicationName, CancellationToken cancellationToken = default);
    }
}
