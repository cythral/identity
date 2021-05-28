using System.Threading;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Auth
{
    [ScopedService]
    public interface ILinkStartUrlService
    {
        /// <summary>
        /// Gets the Account Link Start URL for a given login provider.
        /// </summary>
        /// <param name="providerName">Name of the provider to get a start link for.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting URL if found, or null if not.</returns>
        Task<string?> GetLinkStartUrlForProvider(string providerName, CancellationToken cancellationToken = default);
    }
}
