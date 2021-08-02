using System;
using System.Net;

namespace Brighid.Identity
{
    public class HttpStatusCodeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpStatusCodeException" /> class.
        /// </summary>
        /// <param name="status">The status code to return.</param>
        /// <param name="message">The message to return.</param>
        public HttpStatusCodeException(HttpStatusCode status, string message)
            : base(message)
        {
            Status = status;
        }

        /// <summary>
        /// Gets the status code to respond with.
        /// </summary>
        public HttpStatusCode Status { get; private set; }
    }
}
