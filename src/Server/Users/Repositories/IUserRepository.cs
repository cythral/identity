using System;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Users
{
    [ScopedService]
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<User?> FindByLogin(string loginProvider, string providerKey, params string[] embeds);
    }
}
