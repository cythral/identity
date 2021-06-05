using System;

#pragma warning disable CA1032

namespace Brighid.Identity.Roles
{
    public class PrincipalAlreadyHasRoleException : Exception, IValidationException
    {
        public PrincipalAlreadyHasRoleException(string principalName, string roleName)
            : base($"Principal {principalName} already has role {roleName}.")
        {
            PrincipalName = principalName;
            RoleName = roleName;
        }

        public string PrincipalName { get; init; }

        public string RoleName { get; init; }
    }
}
