using System;
using System.Linq;
using System.Numerics;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Users
{
    public class DefaultUserRepository : Repository<User, Guid>, IUserRepository
    {
        public DefaultUserRepository(DatabaseContext context) : base(context) { }
    }
}
