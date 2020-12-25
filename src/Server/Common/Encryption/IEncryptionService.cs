using System.Threading;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity
{
    /// <summary>
    /// Describes a service to encrypt values.
    /// </summary>
    [ScopedService]
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts a value.
        /// </summary>
        /// <param name="plaintext">The value to be encrypted.</param>
        /// <returns>The encrypted value.</returns>
        Task<string> Encrypt(string plaintext, CancellationToken cancellationToken = default);
    }
}