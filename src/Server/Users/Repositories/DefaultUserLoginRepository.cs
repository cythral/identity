using System;

namespace Brighid.Identity.Users
{
    public class DefaultUserLoginRepository : Repository<UserLogin, Guid>, IUserLoginRepository
    {
        public DefaultUserLoginRepository(DatabaseContext context) : base(context) { }
    }
}
