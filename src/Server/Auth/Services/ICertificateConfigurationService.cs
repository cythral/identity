using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity.Auth
{
    /// <summary>
    /// Service to manage the active certificate configuration.
    /// </summary>
    public interface ICertificateConfigurationService
    {
        /// <summary>
        /// Gets the active certificate configuration.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The active configuration.</returns>
        Task<Configuration> GetConfiguration(CancellationToken cancellationToken = default);
    }
}
