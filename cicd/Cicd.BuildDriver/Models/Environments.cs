using System.Collections.Generic;

namespace Brighid.Identity.Cicd.Utils
{
    /// <summary>
    /// Represents an environment being deployed to.
    /// </summary>
    public class Environments
    {
        /// <summary>
        /// Gets or sets the dev parameters.
        /// </summary>
        public Dictionary<string, string> Dev { get; set; } = new();

        /// <summary>
        /// Gets or sets the prod parameters.
        /// </summary>
        public Dictionary<string, string> Prod { get; set; } = new();
    }
}
