using System;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Users
{
    [ScopedService]
    public interface IUserService
    {
        Task<User> Create(string username, string password, string? role = null);

        Task<UserLogin> CreateLogin(Guid id, UserLogin loginInfo);
    }
}
