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

        public DefaultApplicationService(
           IApplicationRepository appRepository,
           OpenIddictApplicationManager<OpenIddictApplication> appManager,
           GenerateRandomString generateRandomString
       )
        {
            this.appRepository = appRepository;
            this.appManager = appManager;
            this.generateRandomString = generateRandomString;
        }

        public async Task<OpenIddictApplication> Create(Application application)
        {
            await appRepository.Add(application);
            return await appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = $"{application.Name}@identity.brigh.id",
                ClientSecret = generateRandomString(128),
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials
                }
            });
        }

        public async Task<OpenIddictApplication> Update(Application application, bool regenerateClientSecret = false)
        {
            await appRepository.Save(application);
            var client = await appManager.FindByClientIdAsync($"{application.Name}@identity.brigh.id");

            if (regenerateClientSecret)
            {
                client.ClientSecret = generateRandomString(128);
                await appManager.UpdateAsync(client);
            }

            return client;
        }

        public async Task<OpenIddictApplication> Delete(Application application)
        {
            await appRepository.Remove(application.Name);

            var client = await appManager.FindByClientIdAsync($"{application.Name}@identity.brigh.id");
            await appManager.DeleteAsync(client);

            return client;
        }
    }
}
