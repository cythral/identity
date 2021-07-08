using System.Collections.Generic;

using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.IdentityModel.Tokens;

using NUnit.Framework;

using OpenIddict.Server;
using OpenIddict.Validation;

// using OpenIddict.Validation;
namespace Brighid.Identity.Auth
{
    public class OptionsCertificateManagerTests
    {
        [TestFixture]
        [Category("Unit")]
        public class UpdateServerSigningCertificateTests
        {
            [Test]
            [Auto]
            public void ShouldAddTheNewSigningCredentialToTheServerOptions(
                SigningCredentials signingCredentials,
                [Frozen] OpenIddictServerOptions serverOptions,
                [Target] OptionsCertificateManager manager
            )
            {
                manager.UpdateCertificates(signingCredentials);

                serverOptions.SigningCredentials.Should().Contain(signingCredentials);
            }

            [Test]
            [Auto]
            public void ShouldRemoveOldSigningCredentialsFromTheServerOptions(
                SigningCredentials oldSigningCredentials,
                SigningCredentials signingCredentials,
                [Frozen] OpenIddictServerOptions serverOptions,
                [Target] OptionsCertificateManager manager
            )
            {
                serverOptions.SigningCredentials.Add(oldSigningCredentials);
                manager.UpdateCertificates(signingCredentials);

                serverOptions.SigningCredentials.Should().NotContain(oldSigningCredentials);
            }

            [Test]
            [Auto]
            public void ShouldUpdateServerIssuerKeys(
                SigningCredentials oldSigningCredentials,
                SigningCredentials signingCredentials,
                [Frozen] OpenIddictServerOptions serverOptions,
                [Target] OptionsCertificateManager manager
            )
            {
                serverOptions.TokenValidationParameters.IssuerSigningKeys = new List<SecurityKey> { oldSigningCredentials.Key };
                manager.UpdateCertificates(signingCredentials);

                serverOptions.TokenValidationParameters.IssuerSigningKeys.Should().BeEquivalentTo(new[] { signingCredentials.Key });
            }

            [Test]
            [Auto]
            public void ShouldUpdateValidationIssuerKeys(
                SigningCredentials oldSigningCredentials,
                SigningCredentials signingCredentials,
                [Frozen] OpenIddictValidationOptions validationOptions,
                [Target] OptionsCertificateManager manager
            )
            {
                validationOptions.TokenValidationParameters.IssuerSigningKeys = new List<SecurityKey> { oldSigningCredentials.Key };
                manager.UpdateCertificates(signingCredentials);

                validationOptions.TokenValidationParameters.IssuerSigningKeys.Should().BeEquivalentTo(new[] { signingCredentials.Key });
            }
        }
    }
}
