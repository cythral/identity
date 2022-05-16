using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Users
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<User?> FindByLogin(string loginProvider, string providerKey, CancellationToken cancellationToken = default);

        Task<IEnumerable<Role>?> FindRolesById(Guid id, CancellationToken cancellationToken = default);
    }
}
