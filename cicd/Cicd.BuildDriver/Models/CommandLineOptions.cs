namespace Brighid.Identity.Cicd.BuildDriver
{
    /// <summary>
    /// Options presented on the command line.
    /// </summary>
    public class CommandLineOptions
    {
        /// <summary>
        /// Gets or sets the version to build.
        /// </summary>
        public string Version { get; set; } = string.Empty;
    }
}
