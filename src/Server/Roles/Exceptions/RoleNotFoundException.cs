using System;

#pragma warning disable CA1032

namespace Brighid.Identity.Roles
{
    public class RoleNotFoundException : Exception, IValidationException
    {
        public RoleNotFoundException(string name)
            : base($"Role {name} was not found.")
        {
            RoleName = name;
        }

        public string RoleName { get; init; }
    }
}
