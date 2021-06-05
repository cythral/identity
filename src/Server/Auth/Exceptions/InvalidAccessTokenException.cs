using System;

namespace Brighid.Identity.Auth
{
    /// <summary>
    /// Exception that is thrown when given an invalid access token.
    /// </summary>
    public class InvalidAccessTokenException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAccessTokenException" /> class.
        /// </summary>
        public InvalidAccessTokenException()
            : base("The given access token was invalid.")
        {
        }
    }
}
