using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

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
