using System;

#pragma warning disable CA1032

namespace Brighid.Identity.Users
{
    public class UserLoginAlreadyExistsException : Exception
    {
        public UserLoginAlreadyExistsException(UserLogin login)
            : base($"User with ID {login.User.Id} already has an account linked for {login.LoginProvider}.")
        {
            Login = login;
        }

        public UserLogin Login { get; init; }
    }
}
