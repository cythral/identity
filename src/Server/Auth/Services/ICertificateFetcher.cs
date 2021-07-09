using System.Threading;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Tokens;

namespace Brighid.Identity.Auth
{
    /// <summary>
    /// Service that fetches certificates.
    /// </summary>
    public interface ICertificateFetcher
    {
        /// <summary>
        /// Fetch a certificate by its hash from the given <paramref name="bucket" />.
        /// </summary>
        /// <param name="bucket">The bucket that the certificate is located in.</param>
        /// <param name="hash">The hash of the certificate to fetch.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting certificate.</returns>
        Task<SigningCredentials?> FetchCertificate(string bucket, string hash, CancellationToken cancellationToken = default);
    }
}
