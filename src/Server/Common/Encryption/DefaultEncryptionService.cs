using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;

using Microsoft.Extensions.Options;

namespace Brighid.Identity
{
    /// <summary>
    /// Default decryption service - uses KMS to decrypt values.
    /// </summary>
    public class DefaultEncryptionService : IEncryptionService
    {
        private readonly IAmazonKeyManagementService kmsClient;
        private readonly EncryptionOptions encryptionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEncryptionService" /> class.
        /// </summary>
        /// <param name="kmsClient">The KMS Client to use when decrypting values.</param>
        /// <param name="encryptionOptions">Options to use when encrypting strings.</param>
        public DefaultEncryptionService(IAmazonKeyManagementService kmsClient, IOptions<EncryptionOptions> encryptionOptions)
        {
            this.kmsClient = kmsClient;
            this.encryptionOptions = encryptionOptions.Value;
        }

        /// <summary>
        /// Decrypts a value and returns it as a plaintext string.
        /// </summary>
        /// <param name="plaintext">The plaintext to encrypt.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The plaintext encrypted value.</returns>
        public virtual async Task<string> Encrypt(string plaintext, CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream();
            var bytes = Encoding.UTF8.GetBytes(plaintext);
            await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);

            var request = new EncryptRequest { Plaintext = stream, KeyId = encryptionOptions.KmsKeyId };
            var response = await kmsClient.EncryptAsync(request, cancellationToken).ConfigureAwait(false);
            var encryptedBytes = response.CiphertextBlob.ToArray();

            return Convert.ToBase64String(encryptedBytes);
        }
    }
}
