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
        private readonly GenerateRandomString generateRandomString;
        private readonly IEncryptionService encryptionService;

        public DefaultApplicationService(
           IApplicationRepository appRepository,
           OpenIddictApplicationManager<OpenIddictApplication> appManager,
           GenerateRandomString generateRandomString,
           IEncryptionService encryptionService
       )
        {
            this.appRepository = appRepository;
            this.appManager = appManager;
            this.generateRandomString = generateRandomString;
            this.encryptionService = encryptionService;
        }

        public async Task<ApplicationCredentials> Create(Application application)
        {
            await appRepository.Add(application).ConfigureAwait(false);
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = $"{application.Name}@identity.brigh.id",
                ClientSecret = generateRandomString(128),
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Scopes.Roles
                }
            };

            await appManager.CreateAsync(descriptor).ConfigureAwait(false);

            var clientId = descriptor.ClientId;
            var clientSecret = await encryptionService.Encrypt(descriptor.ClientSecret).ConfigureAwait(false);

            return new ApplicationCredentials(clientId, clientSecret);
        }

        public async Task<ApplicationCredentials> Update(Application application)
        {
            await appRepository.Save(application).ConfigureAwait(false);
            var client = await appManager.FindByClientIdAsync($"{application.Name}@identity.brigh.id").ConfigureAwait(false);
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = client.ClientId,
                ClientSecret = generateRandomString(128),
            };

            client.ClientSecret = descriptor.ClientSecret;
            await appManager.UpdateAsync(client).ConfigureAwait(false);

            var clientId = descriptor.ClientId;
            var clientSecret = await encryptionService.Encrypt(descriptor.ClientSecret).ConfigureAwait(false);

            return new ApplicationCredentials(clientId, clientSecret);
        }

        public async Task<ApplicationCredentials> Delete(Application application)
        {
            await appRepository.Remove(application.Id).ConfigureAwait(false);

            var client = await appManager.FindByClientIdAsync($"{application.Name}@identity.brigh.id").ConfigureAwait(false);
            await appManager.DeleteAsync(client).ConfigureAwait(false);


            return new ApplicationCredentials(client.ClientId, "");
        }
    }
}
