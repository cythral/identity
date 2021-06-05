using System;

namespace Brighid.Identity.Auth
{
    public class MissingAuthParameterException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingAuthParameterException" /> class.
        /// </summary>
        /// <param name="parameter">The missing auth parameter.</param>
        /// <param name="grantType">The grant type the auth parameter is missing from.</param>
        public MissingAuthParameterException(string parameter, string grantType)
            : base($"Parameter '{parameter}' must be set for grant type {grantType}.")
        {
        }

        /// <summary>
        /// Gets the name of the missing parameter.
        /// </summary>
        public string Parameter { get; }
    }
}
