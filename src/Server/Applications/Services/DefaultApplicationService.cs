using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationService : IApplicationService
    {
        private readonly OpenIddictApplicationManager<OpenIddictApplication> appManager;
        private readonly IApplicationRepository appRepository;
        private readonly IApplicationRoleService appRoleService;
        private readonly GenerateRandomString generateRandomString;
        private readonly IEncryptionService encryptionService;

        public DefaultApplicationService(
           IApplicationRepository appRepository,
           IApplicationRoleService appRoleService,
           OpenIddictApplicationManager<OpenIddictApplication> appManager,
           GenerateRandomString generateRandomString,
           IEncryptionService encryptionService
       )
        {
            this.appRepository = appRepository;
            this.appRoleService = appRoleService;
            this.appManager = appManager;
            this.generateRandomString = generateRandomString;
            this.encryptionService = encryptionService;
        }

        public Guid GetPrimaryKey(Application application) => application.Id;

        public async Task<Application> Create(Application application)
        {
            var secret = generateRandomString(128);
            var roles = application.Roles;
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = application.Id.ToString(),
                ClientSecret = secret,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Scopes.Roles
                }
            };

            application.EncryptedSecret = await encryptionService.Encrypt(secret).ConfigureAwait(false);
            application.Secret = secret;
            application.Roles = new List<ApplicationRole>();

            await appRoleService.UpdatePrincipalRoles(application, roles);
            await appRepository.Add(application).ConfigureAwait(false);
            await appManager.CreateAsync(descriptor).ConfigureAwait(false);

            return application;
        }

        public async Task<Application> UpdateById(Guid id, Application application)
        {
            var existingApp = await appRepository.FindById(id, "Roles.Role");
            if (existingApp == null)
            {
                throw new UpdateApplicationException($"Application with ID={id} does not exist.");
            }

            var serialChanged = existingApp.Serial != application.Serial;
            existingApp.Name = application.Name;
            existingApp.Description = application.Description;
            existingApp.Serial = application.Serial;

            if (serialChanged)
            {
                var secret = generateRandomString(128);
                var encryptedSecret = await encryptionService.Encrypt(secret).ConfigureAwait(false);
                var client = await appManager.FindByClientIdAsync(id.ToString()).ConfigureAwait(false);

                existingApp.EncryptedSecret = encryptedSecret;
                existingApp.Secret = secret;
                client.ClientSecret = secret;

                await appManager.UpdateAsync(client).ConfigureAwait(false);
            }

            await appRoleService.UpdatePrincipalRoles(existingApp, application.Roles);
            await appRepository.Save(existingApp).ConfigureAwait(false);

            return existingApp;
        }

        public async Task<Application> DeleteById(Guid id)
        {
            var result = await appRepository.Remove(id).ConfigureAwait(false);
            var client = await appManager.FindByClientIdAsync(id.ToString()).ConfigureAwait(false);
            await appManager.DeleteAsync(client).ConfigureAwait(false);

            return result;
        }
    }
}
