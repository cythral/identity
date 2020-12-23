using Microsoft.AspNetCore.Authorization;

namespace Brighid.Identity
{
    public class PoliciesAttribute : AuthorizeAttribute
    {
        public PoliciesAttribute(string[] policies)
        {
            Policies = policies;
            Policy = string.Join(',', policies);
        }

        public string[] Policies { get; }
    }
}
