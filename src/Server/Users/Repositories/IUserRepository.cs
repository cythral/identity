using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Users
{
    [ScopedService]
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<User?> FindByLogin(string loginProvider, string providerKey, params string[] embeds);

        Task<IEnumerable<Role>?> FindRolesById(Guid id, CancellationToken cancellationToken = default);
    }
}
