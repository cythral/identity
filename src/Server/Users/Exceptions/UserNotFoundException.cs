using System;

#pragma warning disable CA1032

namespace Brighid.Identity.Users
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(Guid id) : base($"User with ID {id} was not found.")
        {
            UserId = id;
        }

        public Guid UserId { get; init; }
    }
}
