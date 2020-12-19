using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

using static NSubstitute.Arg;

#pragma warning disable IDE0060, CA1801

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationServiceTests
    {
        public class CreateTests
        {
            [Test, Auto]
            public async Task ShouldCreateANewApplication(
                Application application,
                [Frozen, Substitute] DatabaseContext _,
                [Frozen, Substitute] IApplicationRepository applicationRepository,
                [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> _appManager,
                [Target] DefaultApplicationService applicationService
            )
            {
                await applicationService.Create(application);
                await applicationRepository.Received().Add(application);
            }

            [Test, Auto]
            public async Task ShouldSaveApplicationDetailsToTheDatabaseAndReturnWithSecret(
                string clientSecret,
                Application application,
                [Frozen, Substitute] DatabaseContext _,
                [Frozen, Substitute] GenerateRandomString generateRandomString,
                [Frozen, Substitute] IEncryptionService encryptionService,
                [Frozen, Substitute] IApplicationRepository appRepository,
                [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> _appManager,
                [Target] DefaultApplicationService applicationService
            )
            {
                encryptionService.Encrypt(Any<string>()).Returns(x => $"encrypted {x.ArgAt<string>(0)}");
                generateRandomString(Any<int>()).Returns(clientSecret);

                var result = await applicationService.Create(application);

                result.Id.Should().Be(application.Id);
                result.EncryptedSecret.Should().Be($"encrypted {clientSecret}");
                result.Secret.Should().Be(clientSecret);

                await appRepository.Received().Add(Is<Application>(app =>
                    app.Id == application.Id &&
                    app.EncryptedSecret == $"encrypted {clientSecret}"
                ));

                await encryptionService.Received().Encrypt(Is(clientSecret));
                generateRandomString.Received()(Is(128));
            }

            [Test, Auto]
            public async Task ShouldCreateAnOpenIdDictApplication(
                string clientSecret,
                Application application,
                [Frozen, Substitute] DatabaseContext _,
                [Frozen, Substitute] GenerateRandomString generateRandomString,
                [Frozen, Substitute] IEncryptionService encryptionService,
                [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> appManager,
                [Target] DefaultApplicationService applicationService
            )
            {
                encryptionService.Encrypt(Any<string>()).Returns(x => $"encrypted {x.ArgAt<string>(0)}");
                generateRandomString(Any<int>()).Returns(clientSecret);

                var result = await applicationService.Create(application);

                await appManager.Received().CreateAsync(Is<OpenIddictApplicationDescriptor>(req =>
                    req.ClientId == application.Id.ToString() &&
                    req.ClientSecret == clientSecret &&
                    req.Permissions.Contains(OpenIddictConstants.Permissions.Endpoints.Token) &&
                    req.Permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials)
                ));

                generateRandomString.Received()(Is(128));
            }
        }


        public class UpdateTests
        {
            [Test, Auto]
            public async Task ShouldFetchExistingAppWithRoleEmbedded(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationRepository repository,
                [Target] DefaultApplicationService service
            )
            {
                repository.GetById(Any<Guid>(), Any<string>()).Returns<Application>(x => throw new Exception());

                Func<Task<Application>> func = () => service.Update(id, application);

                await func.Should().ThrowAsync<Exception>();
                await repository.Received().GetById(Is(id), Is("Roles.Role"));
            }

            [Test, Auto]
            public async Task ShouldThrowIfEntityDoesntExist(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationRepository repository,
                [Target] DefaultApplicationService service
            )
            {
                repository.GetById(Any<Guid>(), Any<string>()).Returns((Application)null!);

                Func<Task<Application>> func = () => service.Update(id, application);

                await func.Should().ThrowAsync<UpdateApplicationException>();
            }

            [Test, Auto]
            public async Task ShouldUpdateApplicationRoles(
                ulong serial,
                Guid id,
                Application existingApp,
                Application application,
                [Frozen, Substitute] IApplicationRepository repository,
                [Frozen, Substitute] IApplicationRoleService roleService,
                [Target] DefaultApplicationService service
            )
            {
                existingApp.Serial = serial;
                application.Serial = serial;
                repository.GetById(Any<Guid>(), Any<string>()).Returns(existingApp);
                await service.Update(id, application);

                roleService.Received().UpdateApplicationRoles(Is(existingApp), Is(application.Roles));
            }

            [Test, Auto]
            public async Task ShouldSaveAppWithUpdatedName(
                ulong serial,
                Guid id,
                Application existingApp,
                Application application,
                [Frozen, Substitute] IApplicationRepository repository,
                [Frozen, Substitute] IApplicationRoleService roleService,
                [Target] DefaultApplicationService service
            )
            {
                existingApp.Serial = serial;
                application.Serial = serial;
                repository.GetById(Any<Guid>(), Any<string>()).Returns(existingApp);
                await service.Update(id, application);

                await repository.Received().Save(Is<Application>(app =>
                    app.Name == application.Name
                ));
            }

            [Test, Auto]
            public async Task ShouldSaveAppWithUpdatedDescription(
                ulong serial,
                Guid id,
                Application existingApp,
                Application application,
                [Frozen, Substitute] IApplicationRepository repository,
                [Frozen, Substitute] IApplicationRoleService roleService,
                [Target] DefaultApplicationService service
            )
            {
                existingApp.Serial = serial;
                application.Serial = serial;
                repository.GetById(Any<Guid>(), Any<string>()).Returns(existingApp);
                await service.Update(id, application);

                await repository.Received().Save(Is<Application>(app =>
                    app.Description == application.Description
                ));
            }

            [Test, Auto]
            public async Task ShouldSaveAppWithUpdatedSerial(
                Guid id,
                Application existingApp,
                Application application,
                [Frozen, Substitute] IApplicationRepository repository,
                [Frozen, Substitute] IApplicationRoleService roleService,
                [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> applicationManager,
                [Target] DefaultApplicationService service
            )
            {
                application.Serial.Should().NotBe(existingApp.Serial);

                repository.GetById(Any<Guid>(), Any<string>()).Returns(existingApp);
                await service.Update(id, application);

                var openIddictApplication = new OpenIddictApplication { ClientId = id.ToString() };
                applicationManager.FindByClientIdAsync(Any<string>()).Returns(openIddictApplication);

                await repository.Received().Save(Is<Application>(app =>
                    app.Serial == application.Serial
                ));
            }

            [Test, Auto]
            public async Task ShouldSaveAppWithUpdatedEncryptedSecret_IfSerialChanged(
                string secret,
                string encryptedSecret,
                Guid id,
                Application existingApp,
                Application application,
                [Frozen, Substitute] GenerateRandomString generateRandomString,
                [Frozen, Substitute] IEncryptionService encryptionService,
                [Frozen, Substitute] IApplicationRepository repository,
                [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> applicationManager,
                [Frozen, Substitute] IApplicationRoleService roleService,
                [Target] DefaultApplicationService service
            )
            {
                application.Serial.Should().NotBe(existingApp.Serial);

                generateRandomString(Any<int>()).Returns(secret);
                encryptionService.Encrypt(Any<string>()).Returns(encryptedSecret);
                repository.GetById(Any<Guid>(), Any<string>()).Returns(existingApp);

                var openIddictApplication = new OpenIddictApplication { ClientId = id.ToString() };
                applicationManager.FindByClientIdAsync(Any<string>()).Returns(openIddictApplication);

                await service.Update(id, application);

                generateRandomString.Received()(Is(128));
                await encryptionService.Received().Encrypt(Is(secret));
                await repository.Received().Save(Is<Application>(app =>
                    app.EncryptedSecret == encryptedSecret
                ));
            }

            [Test, Auto]
            public async Task ShouldNotSaveAppWithUpdatedEncryptedSecret_IfSerialDidNotChange(
                string secret,
                string encryptedSecret,
                Guid id,
                Application existingApp,
                Application application,
                [Frozen, Substitute] GenerateRandomString generateRandomString,
                [Frozen, Substitute] IEncryptionService encryptionService,
                [Frozen, Substitute] IApplicationRepository repository,
                [Frozen, Substitute] IApplicationRoleService roleService,
                [Target] DefaultApplicationService service
            )
            {
                existingApp.Serial = 0;
                application.Serial = 0;
                generateRandomString(Any<int>()).Returns(secret);
                encryptionService.Encrypt(Any<string>()).Returns(encryptedSecret);

                repository.GetById(Any<Guid>(), Any<string>()).Returns(existingApp);
                await service.Update(id, application);

                generateRandomString.DidNotReceive()(Is(128));
                await encryptionService.DidNotReceive().Encrypt(Is(secret));
                await repository.DidNotReceive().Save(Is<Application>(app =>
                    app.EncryptedSecret == encryptedSecret
                ));
            }

            [Test, Auto]
            public async Task ShouldReturnAppWithUpdatedSecret_IfSerialChanged(
                string secret,
                string encryptedSecret,
                Guid id,
                Application existingApp,
                Application application,
                [Frozen, Substitute] GenerateRandomString generateRandomString,
                [Frozen, Substitute] IEncryptionService encryptionService,
                [Frozen, Substitute] IApplicationRepository repository,
                [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> applicationManager,
                [Frozen, Substitute] IApplicationRoleService roleService,
                [Target] DefaultApplicationService service
            )
            {
                existingApp.Serial = 0;
                application.Serial = 1;
                generateRandomString(Any<int>()).Returns(secret);
                encryptionService.Encrypt(Any<string>()).Returns(encryptedSecret);

                var openIddictApplication = new OpenIddictApplication { ClientId = id.ToString() };
                applicationManager.FindByClientIdAsync(Any<string>()).Returns(openIddictApplication);

                repository.GetById(Any<Guid>(), Any<string>()).Returns(existingApp);
                var result = await service.Update(id, application);

                result.Secret.Should().Be(secret);
            }

            [Test, Auto]
            public async Task ShouldNotReturnAppWithUpdatedSecret_IfSerialDidNotChange(
                string secret,
                string encryptedSecret,
                Guid id,
                Application existingApp,
                Application application,
                [Frozen, Substitute] GenerateRandomString generateRandomString,
                [Frozen, Substitute] IEncryptionService encryptionService,
                [Frozen, Substitute] IApplicationRepository repository,
                [Frozen, Substitute] IApplicationRoleService roleService,
                [Target] DefaultApplicationService service
            )
            {
                existingApp.Serial = 0;
                application.Serial = 0;
                generateRandomString(Any<int>()).Returns(secret);
                encryptionService.Encrypt(Any<string>()).Returns(encryptedSecret);

                repository.GetById(Any<Guid>(), Any<string>()).Returns(existingApp);
                var result = await service.Update(id, application);

                result.Secret.Should().NotBe(secret);
            }

            [Test, Auto]
            public async Task ShouldUpdateOpenIdAppClientSecret_IfSerialChanged(
                string secret,
                Guid id,
                Application existingApp,
                Application application,
                [Frozen, Substitute] GenerateRandomString generateRandomString,
                [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> applicationManager,
                [Frozen, Substitute] IApplicationRepository repository,
                [Frozen, Substitute] IApplicationRoleService roleService,
                [Target] DefaultApplicationService service
            )
            {
                existingApp.Serial = 0;
                application.Serial = 1;
                generateRandomString(Any<int>()).Returns(secret);

                var openIddictApplication = new OpenIddictApplication { ClientId = id.ToString() };
                applicationManager.FindByClientIdAsync(Any<string>()).Returns(openIddictApplication);

                repository.GetById(Any<Guid>(), Any<string>()).Returns(existingApp);
                var result = await service.Update(id, application);

                await applicationManager.Received().FindByClientIdAsync(Is(id.ToString()));
                await applicationManager.Received().UpdateAsync(Is<OpenIddictApplication>(app =>
                    app.ClientSecret == secret
                ));
            }
        }

        public class DeleteTests
        {
            [Test, Auto]
            public async Task ShouldDeleteTheApplication(
                Guid id,
                [Frozen, Substitute] IApplicationRepository repository,
                [Target] DefaultApplicationService service
            )
            {
                await service.Delete(id);

                await repository.Received().Remove(Is(id));
            }

            [Test, Auto]
            public async Task ShouldDeleteTheApplicationCredentials(
                Guid id,
                [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> applicationManager,
                [Target] DefaultApplicationService service
            )
            {
                var openIddictApplication = new OpenIddictApplication { ClientId = id.ToString() };
                applicationManager.FindByClientIdAsync(Any<string>()).Returns(openIddictApplication);

                await service.Delete(id);

                await applicationManager.Received().FindByClientIdAsync(Is(id.ToString()));
                await applicationManager.Received().DeleteAsync(Is(openIddictApplication));
            }

            [Test, Auto]
            public async Task ShouldReturnTheApplication(
                Guid id,
                Application application,
                [Frozen, Substitute] IApplicationRepository repository,
                [Target] DefaultApplicationService service
            )
            {
                repository.Remove(Any<Guid>()).Returns(application);

                var result = await service.Delete(id);

                result.Should().Be(application);
            }
        }

        // [Test, Auto]
        // public async Task Update_ShouldUpdateTheClientApp(
        //     string clientId,
        //     string clientSecret,
        //     Application application,
        //     [Frozen, Substitute] GenerateRandomString generateRandomString,
        //     [Frozen] IEncryptionService encryptionService,
        //     [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> applicationManager,
        //     [Target] DefaultApplicationService applicationService
        // )
        // {
        //     var client = new OpenIddictApplication { ClientId = clientId };
        //     encryptionService.Encrypt(Any<string>()).Returns(x => $"encrypted {x.ArgAt<string>(0)}");
        //     generateRandomString(Any<int>()).Returns(clientSecret);
        //     applicationManager.FindByClientIdAsync(Any<string>()).Returns(client);

        //     var result = await applicationService.Update(application);

        //     result.ClientId.Should().Be(clientId);
        //     result.ClientSecret.Should().Be($"encrypted {clientSecret}");

        //     await applicationManager.Received().FindByClientIdAsync(Is(application.Name + "@identity.brigh.id"));
        //     await applicationManager.Received().UpdateAsync(Is<OpenIddictApplication>(req =>
        //         req.ClientId == clientId &&
        //         req.ClientSecret == clientSecret
        //     ));
        //     generateRandomString.Received()(Is(128));
        // }

        // [Test, Auto]
        // public async Task Delete_ShouldDeleteTheApplication(
        //     Application application,
        //     [Frozen] IApplicationRepository applicationRepository,
        //     [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> _,
        //     [Target] DefaultApplicationService applicationService
        // )
        // {
        //     await applicationService.Delete(application);

        //     await applicationRepository.Received().Remove(application.Id);
        // }

        // [Test, Auto]
        // public async Task Delete_ShouldDeleteTheApplication(
        //     OpenIddictApplication clientApp,
        //     Application application,
        //     [Frozen, Substitute] OpenIddictApplicationManager<OpenIddictApplication> applicationManager,
        //     [Target] DefaultApplicationService applicationService
        // )
        // {
        //     applicationManager.FindByClientIdAsync(Any<string>()).Returns(clientApp);

        //     var result = await applicationService.Delete(application);

        //     result.Should().NotBeNull();

        //     await applicationManager.Received().FindByClientIdAsync(Is(application.Name + "@identity.brigh.id"));
        //     await applicationManager.Received().DeleteAsync(Is(clientApp));
        // }
    }
}
