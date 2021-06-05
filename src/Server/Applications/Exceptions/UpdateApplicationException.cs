using System;

namespace Brighid.Identity.Applications
{
    public class UpdateApplicationException : Exception, IValidationException
    {
        public UpdateApplicationException()
        {
        }

        public UpdateApplicationException(string message)
            : base(message)
        {
        }

        public UpdateApplicationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
