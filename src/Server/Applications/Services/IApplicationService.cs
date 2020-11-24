using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Applications
{
    [ScopedService]
    public interface IApplicationService
    {
        Task<ApplicationCredentials> Create(Application application);
        Task<ApplicationCredentials> Update(Application application);
        Task<ApplicationCredentials> Delete(Application application);
    }
}
