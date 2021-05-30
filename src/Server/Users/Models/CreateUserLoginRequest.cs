using System;

namespace Brighid.Identity.Users
{
    public class CreateUserLoginRequest : UserLogin
    {
        private new Guid Id { get; set; } = Guid.NewGuid();
    }
}
