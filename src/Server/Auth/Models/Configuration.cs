namespace Brighid.Identity.Auth
{
    /// <summary>
    /// Represents the active certificate configuration.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Gets or sets the bucket name where certificates are located.
        /// </summary>
        public string BucketName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the hash of the active signing certificate.  This is also the key of the certificate's object in S3.
        /// </summary>
        public string ActiveCertificateHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the hash of the inactive signing certificate. This is also the key of the certificate's object in S3.
        /// </summary>
        public string? InactiveCertificateHash { get; set; }
    }
}
