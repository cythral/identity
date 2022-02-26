using System.Collections.Generic;

namespace Brighid.Identity.Cicd.Utils
{
    /// <summary>
    /// Represents an environment-specific configuration file for deploying a CloudFormation template.
    /// </summary>
    public class EnvironmentConfig
    {
        /// <summary>
        /// Gets or sets the parameters to pass to the CloudFormation template for this environment.
        /// </summary>
        public Dictionary<string, string>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets the tags to apply to the CloudFormation template for this environment.
        /// </summary>
        public Dictionary<string, string>? Tags { get; set; }
    }
}
