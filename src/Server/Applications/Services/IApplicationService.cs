using System;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Applications
{
    [ScopedService]
    public interface IApplicationService
    {
        Task<Application> Create(Application application);
        Task<Application> Update(Guid id, Application application);
        Task<Application> Delete(Guid id);
    }
}
