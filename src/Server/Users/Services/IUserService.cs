using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using Microsoft.AspNetCore.Authentication;

namespace Brighid.Identity.Users
{
    [ScopedService]
    public interface IUserService
    {
        Task<User> Create(string username, string password, string? role = null);
        Task<UserLogin> CreateLogin(Guid id, UserLogin loginInfo);
    }
}
