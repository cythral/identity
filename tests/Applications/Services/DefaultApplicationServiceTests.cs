using System;
using System.Net.Http;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Sns;

using FluentAssertions;

using Flurl.Http.Testing;

using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

using static NSubstitute.Arg;

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationServiceTests
    {
        [Test, Auto]
        public async Task Create_ShouldCreateANewApplication(
            Application application,
            [Frozen] IApplicationRepository applicationRepository,
            [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> _,
            [Target] DefaultApplicationService applicationService
        )
        {
            await applicationService.Create(application);
            await applicationRepository.Received().Add(application);
        }

        [Test, Auto]
        public async Task Create_ShouldCreateAnOpenIdDictApplication(
            string clientSecret,
            OpenIddictApplication clientApp,
            [Frozen, Substitute] GenerateRandomString generateRandomString,
            Application application,
            [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> appManager,
            [Target] DefaultApplicationService applicationService
        )
        {
            generateRandomString(Any<int>()).Returns(clientSecret);
            appManager.CreateAsync(Any<OpenIddictApplicationDescriptor>()).Returns(clientApp);

            var result = await applicationService.Create(application);

            result.ClientId.Should().Be(application.Name + "@identity.brigh.id");
            result.ClientSecret.Should().Be(clientSecret);

            await appManager.Received().CreateAsync(Is<OpenIddictApplicationDescriptor>(req =>
                req.ClientId == application.Name + "@identity.brigh.id" &&
                req.ClientSecret == clientSecret &&
                req.Permissions.Contains(OpenIddictConstants.Permissions.Endpoints.Token) &&
                req.Permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials)
            ));

            generateRandomString.Received()(Is(128));
        }

        [Test, Auto]
        public async Task Update_ShouldUpdateTheApplication(
            Application application,
            [Frozen] IApplicationRepository applicationRepository,
            [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> _,
            [Target] DefaultApplicationService applicationService
        )
        {
            await applicationService.Update(application);
            await applicationRepository.Received().Save(application);
        }

        [Test, Auto]
        public async Task Update_ShouldUpdateTheClientApp(
            string clientId,
            string clientSecret,
            Application application,
            [Frozen, Substitute] GenerateRandomString generateRandomString,
            [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> applicationManager,
            [Target] DefaultApplicationService applicationService
        )
        {
            var client = new OpenIddictApplication { ClientId = clientId };
            generateRandomString(Any<int>()).Returns(clientSecret);
            applicationManager.FindByClientIdAsync(Any<string>()).Returns(client);

            var result = await applicationService.Update(application);

            result.ClientId.Should().Be(clientId);
            result.ClientSecret.Should().Be(clientSecret);

            await applicationManager.Received().FindByClientIdAsync(Is(application.Name + "@identity.brigh.id"));
            await applicationManager.Received().UpdateAsync(Is<OpenIddictApplication>(req =>
                req.ClientId == clientId &&
                req.ClientSecret == clientSecret
            ));
            generateRandomString.Received()(Is(128));
        }

        [Test, Auto]
        public async Task Delete_ShouldDeleteTheApplication(
            Application application,
            [Frozen] IApplicationRepository applicationRepository,
            [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> _,
            [Target] DefaultApplicationService applicationService
        )
        {
            await applicationService.Delete(application);

            await applicationRepository.Received().Remove(application.Name);
        }

        [Test, Auto]
        public async Task Delete_ShouldDeleteTheApplication(
            OpenIddictApplication clientApp,
            Application application,
            [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> applicationManager,
            [Target] DefaultApplicationService applicationService
        )
        {
            applicationManager.FindByClientIdAsync(Any<string>()).Returns(clientApp);

            var result = await applicationService.Delete(application);

            result.Should().NotBeNull();

            await applicationManager.Received().FindByClientIdAsync(Is(application.Name + "@identity.brigh.id"));
            await applicationManager.Received().DeleteAsync(Is(clientApp));
        }
    }
}
