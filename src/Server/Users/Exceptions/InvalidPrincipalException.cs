using System;

#pragma warning disable CA1032

namespace Brighid.Identity.Users
{
    public class InvalidPrincipalException : Exception
    {
        public InvalidPrincipalException(string message)
            : base(message)
        {
        }
    }
}
