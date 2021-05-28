namespace Brighid.Identity.Sns
{
    /// <summary>
    /// An SNS message attribute.
    /// </summary>
    public class SnsMessageAttribute
    {
        /// <summary>
        /// Gets the attribute type.
        /// </summary>
        public string Type { get; init; }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        public string Value { get; init; }
    }
}
