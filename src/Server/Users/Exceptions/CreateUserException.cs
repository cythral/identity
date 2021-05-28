using System;

namespace Brighid.Identity.Users
{
    public class CreateUserException : AggregateException
    {
        public CreateUserException()
        {
        }

        public CreateUserException(string message)
            : base(message)
        {
        }

        public CreateUserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CreateUserException(Exception[] innerExceptions)
            : base(innerExceptions)
        {
        }
    }
}
