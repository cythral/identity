using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.IdentityModel.Tokens;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.Auth
{
    public class DefaultCertificateUpdaterTests
    {
        [TestFixture]
        [Category("Unit")]
        public class UpdateCertificatesTests
        {
            [Test]
            [Auto]
            public async Task ShouldFetchCertificateConfiguration(
                [Frozen] ICertificateConfigurationService configService,
                [Target] DefaultCertificateUpdater updater,
                CancellationToken cancellationToken
            )
            {
                await updater.UpdateCertificates(cancellationToken);

                await configService.Received().GetConfiguration(Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldFetchTheActiveCertificate(
                [Frozen] Configuration configuration,
                [Frozen] ICertificateFetcher fetcher,
                [Target] DefaultCertificateUpdater updater,
                CancellationToken cancellationToken
            )
            {
                await updater.UpdateCertificates(cancellationToken);

                await fetcher.Received().FetchCertificate(Is(configuration.BucketName), Is(configuration.ActiveCertificateHash), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldThrowIfTheActiveCertificateIsNull(
                [Frozen] Configuration configuration,
                [Frozen] ICertificateFetcher fetcher,
                [Target] DefaultCertificateUpdater updater,
                CancellationToken cancellationToken
            )
            {
                fetcher.FetchCertificate(Any<string>(), Is(configuration.ActiveCertificateHash), Any<CancellationToken>()).Returns((SigningCredentials)null!);

                Func<Task> func = () => updater.UpdateCertificates(cancellationToken);

                await func.Should().ThrowAsync<Exception>();
            }

            [Test]
            [Auto]
            public async Task ShouldFetchTheInactiveCertificate(
                [Frozen] Configuration configuration,
                [Frozen] ICertificateFetcher fetcher,
                [Target] DefaultCertificateUpdater updater,
                CancellationToken cancellationToken
            )
            {
                await updater.UpdateCertificates(cancellationToken);

                await fetcher.Received().FetchCertificate(Is(configuration.BucketName), Is(configuration.InactiveCertificateHash!), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldNotFetchTheInactiveCertificateIfTheHashIsNull(
                [Frozen] Configuration configuration,
                [Frozen] ICertificateFetcher fetcher,
                [Target] DefaultCertificateUpdater updater,
                CancellationToken cancellationToken
            )
            {
                configuration.InactiveCertificateHash = null;
                await updater.UpdateCertificates(cancellationToken);

                await fetcher.DidNotReceive().FetchCertificate(Is(configuration.BucketName), Is(configuration.InactiveCertificateHash!), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldUpdateTheCertificatesWithActiveAndInactiveCredentials(
                [Frozen] Configuration configuration,
                [Frozen] SigningCredentials inactiveCredentials,
                [Frozen] SigningCredentials activeCredentials,
                [Frozen] ICertificateManager manager,
                [Frozen] ICertificateFetcher fetcher,
                [Target] DefaultCertificateUpdater updater,
                CancellationToken cancellationToken
            )
            {
                fetcher.FetchCertificate(Any<string>(), Is(configuration.ActiveCertificateHash), Any<CancellationToken>()).Returns(inactiveCredentials);
                fetcher.FetchCertificate(Any<string>(), Is(configuration.InactiveCertificateHash!), Any<CancellationToken>()).Returns(activeCredentials);

                await updater.UpdateCertificates(cancellationToken);

                manager.Received().UpdateCertificates(Is(activeCredentials), Is(inactiveCredentials));
            }
        }
    }
}
