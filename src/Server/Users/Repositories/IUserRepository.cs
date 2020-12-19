using System;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Users
{
    [ScopedService]
    public interface IUserRepository : IRepository<User, Guid>
    {
    }
}
