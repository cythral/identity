namespace Brighid.Identity.Sns
{
    /// <summary>
    /// An SNS message attribute.
    /// </summary>
    public class MessageAttribute
    {
        /// <summary>
        /// The attribute type.
        /// </summary>
        public string Type { get; init; }

        /// <summary>
        /// The attribute value.
        /// </summary>
        public string Value { get; init; }
    }
}
