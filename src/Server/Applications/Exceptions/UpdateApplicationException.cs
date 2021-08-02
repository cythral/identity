using System.Net;

namespace Brighid.Identity.Applications
{
    public class UpdateApplicationException : HttpStatusCodeException
    {
        public UpdateApplicationException(string message)
            : base(HttpStatusCode.UnprocessableEntity, message)
        {
        }
    }
}
