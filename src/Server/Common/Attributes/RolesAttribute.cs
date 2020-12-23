using Microsoft.AspNetCore.Authorization;

namespace Brighid.Identity
{
    public class RolesAttribute : AuthorizeAttribute
    {
        public RolesAttribute(string[] roles)
        {
            Roles = string.Join(',', roles);
        }
    }
}
