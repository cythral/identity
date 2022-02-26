using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Amazon.ECR;
using Amazon.ECR.Model;
using Amazon.ECRPublic;

namespace Brighid.Identity.Cicd.Utils
{
    /// <summary>
    /// Utilities for interacting with ECR.
    /// </summary>
    public class EcrUtils
    {
        private readonly IAmazonECR ecr = new AmazonECRClient();
        private readonly IAmazonECRPublic ecrPublic = new AmazonECRPublicClient();

        /// <summary>
        /// Logs into the given ECR repository.
        /// </summary>
        /// <param name="repository">The repository to login to.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task DockerLogin(string repository, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await ecr.GetAuthorizationTokenAsync(new(), cancellationToken);
            var token = response.AuthorizationData.ElementAt(0);
            var passwordBytes = Convert.FromBase64String(token.AuthorizationToken);
            var password = Encoding.ASCII.GetString(passwordBytes)[4..];

            await Login(repository, password, cancellationToken);
        }

        /// <summary>
        /// Logs into ECR Public.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task PublicDockerLogin(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await ecrPublic.GetAuthorizationTokenAsync(new(), cancellationToken);
            var token = response.AuthorizationData;
            var passwordBytes = Convert.FromBase64String(token.AuthorizationToken);
            var password = Encoding.ASCII.GetString(passwordBytes)[4..];

            await Login("public.ecr.aws", password, cancellationToken);
        }

        /// <summary>
        /// Retags an image.
        /// </summary>
        /// <param name="registry">The registry where the repository is located.</param>
        /// <param name="repository">The image repository.</param>
        /// <param name="oldTag">The existing tag of the image to retag.</param>
        /// <param name="newTag">Tag to retag the image with.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task RetagImage(string registry, string repository, string oldTag, string newTag, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var oldTagId = new ImageIdentifier { ImageTag = oldTag };
                var getImageRequest = new BatchGetImageRequest { RegistryId = registry, RepositoryName = repository, ImageIds = new List<ImageIdentifier> { oldTagId } };
                var imageInfo = await ecr.BatchGetImageAsync(getImageRequest, cancellationToken);
                var putImageRequest = new PutImageRequest
                {
                    ImageManifest = imageInfo.Images[0].ImageManifest,
                    RegistryId = registry,
                    RepositoryName = repository,
                    ImageTag = newTag,
                };

                await ecr.PutImageAsync(putImageRequest, cancellationToken);
            }
            catch (ImageAlreadyExistsException)
            {
            }
        }

        private static async Task Login(string repository, string password, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var command = new Command(
                command: "docker login",
                options: new Dictionary<string, object>
                {
                    ["--username"] = "AWS",
                    ["--password-stdin"] = true,
                },
                arguments: new[] { repository }
            );

            await command.RunOrThrowError(
                errorMessage: "Failed to login to ECR.",
                input: password,
                cancellationToken: cancellationToken
            );
        }
    }
}
