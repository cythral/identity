using System;

namespace Brighid.Identity.Roles
{
    public class RoleRequiredException : Exception
    {
        public RoleRequiredException(string role)
            : base($"This operation requires the role {role}")
        {
            Role = role;
        }

        /// <summary>
        /// Gets the role that is required.
        /// </summary>
        public string Role { get; }
    }
}
