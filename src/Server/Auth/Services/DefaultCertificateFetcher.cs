using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Amazon.S3;
using Amazon.S3.Model;

using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Brighid.Identity.Auth
{
    /// <inheritdoc />
    /// <todo>Unit test this.</todo>
    public class DefaultCertificateFetcher : ICertificateFetcher
    {
        private readonly IAmazonS3 s3Client;
        private readonly ILogger<DefaultCertificateFetcher> logger;

        public DefaultCertificateFetcher(
            IAmazonS3 s3Client,
            ILogger<DefaultCertificateFetcher> logger
        )
        {
            this.s3Client = s3Client;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task<SigningCredentials?> FetchCertificate(string bucket, string hash, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetObjectRequest { BucketName = bucket, Key = hash };
                logger.LogInformation("Sending s3:GetObject request: {@request}", request);

                using var response = await s3Client.GetObjectAsync(request, cancellationToken);
                logger.LogInformation("Received s3:GetObject response: {@response}", response);

                using var byteStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(byteStream, cancellationToken);
                await response.ResponseStream.FlushAsync(cancellationToken);
                var cert = new X509Certificate2(byteStream.ToArray());
                var privateKey = cert.GetECDsaPrivateKey();
                var securityKey = new ECDsaSecurityKey(privateKey) { KeyId = hash };
                return new SigningCredentials(securityKey, "ES256");
            }
            catch (Exception exception)
            {
                logger.LogError("Got error attempting to fetch certificate {@h14ash}: {@exception}", hash, exception);
                return null;
            }
        }
    }
}
