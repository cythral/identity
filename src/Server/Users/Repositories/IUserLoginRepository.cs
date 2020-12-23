using System;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Users
{
    [ScopedService]
    public interface IUserLoginRepository : IRepository<UserLogin, Guid>
    {
    }
}
