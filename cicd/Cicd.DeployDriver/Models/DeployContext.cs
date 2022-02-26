using System;
using System.Collections.Generic;

namespace Brighid.Identity.Cicd.DeployDriver
{
    /// <summary>
    /// Context holder for deployment information.
    /// </summary>
    public class DeployContext
    {
        /// <summary>
        /// Gets or sets the name of the stack to deploy.
        /// </summary>
        public string? StackName { get; set; }

        /// <summary>
        /// Gets the name of the deployment change set.
        /// </summary>
        public string ChangeSetName { get; } = "change" + Guid.NewGuid().ToString().Replace("-", string.Empty);

        /// <summary>
        /// Gets the timestamp of the deployment.
        /// </summary>
        public DateTime Timestamp { get; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the S3 URL of the template.
        /// </summary>
        public string? TemplateURL { get; set; }

        /// <summary>
        /// Gets or sets the list of capabilities to use for this deployment.
        /// </summary>
        public List<string> Capabilities { get; set; } = new();

        /// <summary>
        /// Gets or sets the parameters to use for this deployment.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new();

        /// <summary>
        /// Gets or sets the tags to use for this deployment.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new();
    }
}
