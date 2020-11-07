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

        public async Task<OpenIddictApplicationDescriptor> Create(Application application)
        {
            await appRepository.Add(application);
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

            await appManager.CreateAsync(descriptor);
            return descriptor;
        }

        public async Task<OpenIddictApplicationDescriptor> Update(Application application)
        {
            await appRepository.Save(application);
            var client = await appManager.FindByClientIdAsync($"{application.Name}@identity.brigh.id");
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = client.ClientId,
                ClientSecret = generateRandomString(128)
            };

            client.ClientSecret = descriptor.ClientSecret;
            await appManager.UpdateAsync(client);
            return descriptor;
        }

        public async Task<OpenIddictApplicationDescriptor> Delete(Application application)
        {
            await appRepository.Remove(application.Name);

            var client = await appManager.FindByClientIdAsync($"{application.Name}@identity.brigh.id");
            await appManager.DeleteAsync(client);

            return new OpenIddictApplicationDescriptor
            {
                ClientId = client.ClientId
            };
        }
    }
}
