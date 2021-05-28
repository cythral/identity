using System.Threading;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity
{
    /// <summary>
    /// Describes a service to encrypt values.
    /// </summary>
    [SingletonService]
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts a value.
        /// </summary>
        /// <param name="plaintext">The value to be encrypted.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The encrypted value.</returns>
        Task<string> Encrypt(string plaintext, CancellationToken cancellationToken = default);
    }
}
