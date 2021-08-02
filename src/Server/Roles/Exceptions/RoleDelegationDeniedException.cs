using System.Net;

namespace Brighid.Identity.Roles
{
    public class RoleDelegationDeniedException : HttpStatusCodeException, IValidationException
    {
        public RoleDelegationDeniedException(string message)
            : base(HttpStatusCode.UnprocessableEntity, message)
        {
        }
    }
}
