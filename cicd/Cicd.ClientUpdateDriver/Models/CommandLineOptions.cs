using System;

namespace Brighid.Identity.Cicd.ClientUpdateDriver
{
    /// <summary>
    /// Options presented on the command line.
    /// </summary>
    public class CommandLineOptions
    {
        /// <summary>
        /// Gets or sets the location where artifacts are stored.
        /// </summary>
        public Uri? ArtifactsLocation { get; set; }

        /// <summary>
        /// Gets or sets the version to build.
        /// </summary>
        public string Version { get; set; } = string.Empty;
    }
}
