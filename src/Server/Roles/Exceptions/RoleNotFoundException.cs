using System.Net;

#pragma warning disable CA1032

namespace Brighid.Identity.Roles
{
    public class RoleNotFoundException : HttpStatusCodeException, IValidationException
    {
        public RoleNotFoundException(string name)
            : base(HttpStatusCode.UnprocessableEntity, $"Role {name} was not found.")
        {
            RoleName = name;
        }

        public string RoleName { get; init; }
    }
}
