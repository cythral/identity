namespace Brighid.Identity.Cicd.Utils
{
    /// <summary>
    /// Represents a parameter passed to a CloudFormation template.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Gets or sets the dev value for the parameter.
        /// </summary>
        public string Development { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the prod value for the parameter.
        /// </summary>
        public string Production { get; set; } = string.Empty;
    }
}
