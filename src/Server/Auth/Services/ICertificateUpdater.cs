using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity.Auth
{
    public interface ICertificateUpdater
    {
        /// <summary>
        /// Updates the signing certificates used by OpenIddict.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task UpdateCertificates(CancellationToken cancellationToken = default);
    }
}
