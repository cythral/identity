using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

namespace Brighid.Identity.Applications
{
    [ScopedService]
    public interface IApplicationService
    {
        Task<OpenIddictApplication> Create(Application application);
        Task<OpenIddictApplication> Update(Application application, bool regenerateClientSecret = false);
        Task<OpenIddictApplication> Delete(Application application);
    }
}
