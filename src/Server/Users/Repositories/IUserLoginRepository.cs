using System;

namespace Brighid.Identity.Users
{
    public interface IUserLoginRepository : IRepository<UserLogin, Guid>
    {
    }
}
