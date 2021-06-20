using System;
using System.Threading.Tasks;

namespace Brighid.Identity.Users
{
    public interface IUserService
    {
        Task<User> Create(string username, string password, string? role = null);

        Task<UserLogin> CreateLogin(Guid id, UserLogin loginInfo);
    }
}
