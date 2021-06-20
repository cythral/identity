using System;

namespace Brighid.Identity.Users
{
    public class CreateUserLoginRequest : UserLogin
    {
        protected new Guid Id { get; set; } = Guid.NewGuid();
    }
}
