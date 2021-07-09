using System.Collections.Generic;

using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.IdentityModel.Tokens;

using NUnit.Framework;

namespace Brighid.Identity.Auth
{
    public class StartupCertificateManagerTests
    {
        [TestFixture]
        [Category("Unit")]
        public class UpdateServerSigningCertificateTests
        {
            [Test]
            [Auto]
            public void ShouldAddTheNewSigningCredentialToTheStartupCertificates(
                SigningCredentials signingCredentials,
                [Frozen] List<SigningCredentials> startupCertificates,
                [Target] StartupCertificateManager manager
            )
            {
                manager.UpdateCertificates(signingCredentials);

                startupCertificates.Should().BeEquivalentTo(new[] { signingCredentials });
            }
        }
    }
}
