using System;
using System.Threading;
using System.Threading.Tasks;

using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

using AutoFixture.NUnit3;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.Auth
{
    public class DefaultCertificateConfigurationServiceTests
    {
        [TestFixture]
        [Category("Unit")]
        public class GetConfigurationTests
        {
            [Test]
            [Auto]
            public async Task ShouldRetrieveConfigurationFromSsmWithActiveCertificateHash(
                string activeCertificateHash,
                [Frozen] AuthConfig options,
                [Frozen] GetParameterResponse getParameterResponse,
                [Frozen] IAmazonSimpleSystemsManagement ssmClient,
                [Target] DefaultCertificateConfigurationService service,
                CancellationToken cancellationToken
            )
            {
                getParameterResponse.Parameter.Value = $@"{{""ActiveCertificateHash"":""{activeCertificateHash}""}}";

                var result = await service.GetConfiguration(cancellationToken);

                result.ActiveCertificateHash.Should().Be(activeCertificateHash);
                await ssmClient.Received().GetParameterAsync(Is<GetParameterRequest>(req => req.Name == options.CertificateConfigurationParameterName), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldRetrieveConfigurationFromSsmWithInactiveCertificateHash(
                string inactiveCertificateHash,
                [Frozen] AuthConfig options,
                [Frozen] GetParameterResponse getParameterResponse,
                [Frozen] IAmazonSimpleSystemsManagement ssmClient,
                [Target] DefaultCertificateConfigurationService service,
                CancellationToken cancellationToken
            )
            {
                getParameterResponse.Parameter.Value = $@"{{""InactiveCertificateHash"":""{inactiveCertificateHash}""}}";

                var result = await service.GetConfiguration(cancellationToken);

                result.InactiveCertificateHash.Should().Be(inactiveCertificateHash);
                await ssmClient.Received().GetParameterAsync(Is<GetParameterRequest>(req => req.Name == options.CertificateConfigurationParameterName), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldRetrieveConfigurationFromSsmWithBucketName(
                string bucketName,
                [Frozen] AuthConfig options,
                [Frozen] GetParameterResponse getParameterResponse,
                [Frozen] IAmazonSimpleSystemsManagement ssmClient,
                [Target] DefaultCertificateConfigurationService service,
                CancellationToken cancellationToken
            )
            {
                getParameterResponse.Parameter.Value = $@"{{""BucketName"":""{bucketName}""}}";

                var result = await service.GetConfiguration(cancellationToken);

                result.BucketName.Should().Be(bucketName);
                await ssmClient.Received().GetParameterAsync(Is<GetParameterRequest>(req => req.Name == options.CertificateConfigurationParameterName), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldThrowIfParameterIsNull(
                [Frozen] GetParameterResponse getParameterResponse,
                [Target] DefaultCertificateConfigurationService service,
                CancellationToken cancellationToken
            )
            {
                getParameterResponse.Parameter.Value = $@"null";

                Func<Task> func = () => service.GetConfiguration(cancellationToken);

                await func.Should().ThrowAsync<Exception>();
            }
        }
    }
}
