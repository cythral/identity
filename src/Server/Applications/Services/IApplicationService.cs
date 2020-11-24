using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using OpenIddict.Abstractions;

namespace Brighid.Identity.Applications
{
    [ScopedService]
    public interface IApplicationService
    {
        Task<OpenIddictApplicationDescriptor> Create(Application application);
        Task<OpenIddictApplicationDescriptor> Update(Application application);
        Task<OpenIddictApplicationDescriptor> Delete(Application application);
    }
}
