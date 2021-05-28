using System;

namespace Brighid.Identity
{
    public class EntityAlreadyExistsException : Exception
    {
        public EntityAlreadyExistsException()
        {
        }

        public EntityAlreadyExistsException(string message)
            : base(message)
        {
        }

        public EntityAlreadyExistsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
