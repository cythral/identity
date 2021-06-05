using System;

namespace Brighid.Identity.Roles
{
    public class RoleDelegationDeniedException : Exception, IValidationException
    {
        public RoleDelegationDeniedException()
        {
        }

        public RoleDelegationDeniedException(string message)
            : base(message)
        {
        }

        public RoleDelegationDeniedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
