using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Roles
{
    [ScopedService]
    public interface IRoleRepository : IRepository<Role, Guid>
    {
        Task<Role?> FindByName(string name, CancellationToken cancellationToken = default);

        Task<bool> IsAttachedToAPrincipal(Guid id, CancellationToken cancellationToken = default);

        Task<IEnumerable<Role>> FindAllByName(IEnumerable<string> names, CancellationToken cancellationToken = default);
    }
}
