using System;

namespace Brighid.Identity.Auth
{
    public class InvalidCredentialsException : LoginException
    {
        public InvalidCredentialsException(string message) : base(message) { }
        public InvalidCredentialsException(string message, Exception innerException) : base(message, innerException) { }
        public InvalidCredentialsException() : base("Username and/or password were incorrect.") { }
    }
}
