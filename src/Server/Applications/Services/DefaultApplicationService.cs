using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationService : IApplicationService
    {
        private readonly OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> appManager;
        private readonly IApplicationRepository appRepository;
        private readonly GenerateRandomString generateRandomString;
        private readonly IEncryptionService encryptionService;

        public DefaultApplicationService(
           IApplicationRepository appRepository,
           OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> appManager,
           GenerateRandomString generateRandomString,
           IEncryptionService encryptionService
       )
        {
            this.appRepository = appRepository;
            this.appManager = appManager;
            this.generateRandomString = generateRandomString;
            this.encryptionService = encryptionService;
        }

        public Guid GetPrimaryKey(Application application) => application.Id;

        public async Task<Application> Create(Application application)
        {
            var secret = generateRandomString(128);
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = application.Id.ToString(),
                ClientSecret = secret,
            };

            ComputePermissionsFromRoles(descriptor.Permissions, application.Roles);
            application.EncryptedSecret = await encryptionService.Encrypt(secret);
            application.Secret = secret;

            await appRepository.Add(application);
            await appManager.CreateAsync(descriptor);

            return application;
        }

        public async Task<Application> UpdateById(Guid id, Application application)
        {
            var existingApp = await appRepository.FindById(id);
            if (existingApp == null)
            {
                throw new UpdateApplicationException($"Application with ID={id} does not exist.");
            }

            await appRepository.LoadCollection(existingApp, nameof(Application.Roles));

            var serialChanged = existingApp.Serial != application.Serial;
            existingApp.Name = application.Name;
            existingApp.Description = application.Description;
            existingApp.Serial = application.Serial;
            existingApp.Roles = application.Roles;

            var client = await appManager.FindByClientIdAsync(id.ToString());
            var permissions = new HashSet<string>();

            ComputePermissionsFromRoles(permissions, application.Roles);
            client!.Permissions = JsonSerializer.Serialize(permissions);

            if (serialChanged)
            {
                var secret = generateRandomString(128);
                var encryptedSecret = await encryptionService.Encrypt(secret);

                existingApp.EncryptedSecret = encryptedSecret;
                existingApp.Secret = secret;
                client!.ClientSecret = secret;
            }

            await appManager.UpdateAsync(client!);
            await appRepository.Save(existingApp);

            return existingApp;
        }

        public async Task<Application> DeleteById(Guid id)
        {
            var result = await appRepository.Remove(id);
            var client = await appManager.FindByClientIdAsync(id.ToString());
            await appManager.DeleteAsync(client!);

            return result;
        }

        private void ComputePermissionsFromRoles(HashSet<string> permissions, IEnumerable<Role> roles)
        {
            permissions.Clear();
            permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
            permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
            permissions.Add(OpenIddictConstants.Permissions.Scopes.Roles);

            foreach (var role in roles)
            {
                if (Enum.TryParse<BuiltInRole>(role.Name, out var builtInRole))
                {
                    var attributes = typeof(BuiltInRole).GetField(role.Name)!.GetCustomAttributes<AddsPermissionAttribute>();
                    foreach (var attr in attributes)
                    {
                        permissions.Add(attr.Permission);
                    }
                }
            }
        }
    }
}
