using System;

namespace Brighid.Identity.Cicd.DeployDriver
{
    /// <summary>
    /// Options presented on the command line.
    /// </summary>
    public class CommandLineOptions
    {
        /// <summary>
        /// Gets or sets the environment to use for the deployment.
        /// </summary>
        public string? Environment { get; set; }

        /// <summary>
        /// Gets or sets the location where artifacts are stored.
        /// </summary>
        public Uri? ArtifactsLocation { get; set; }
    }
}
