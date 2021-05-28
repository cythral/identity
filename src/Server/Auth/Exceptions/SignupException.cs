using System;

namespace Brighid.Identity.Auth
{
    public class SignupException : Exception
    {
        public SignupException(string message)
            : base(message)
        {
        }

        public SignupException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SignupException()
        {
        }
    }
}
