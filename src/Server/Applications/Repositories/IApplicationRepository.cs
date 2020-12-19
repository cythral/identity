using System;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Applications
{
    [ScopedService]
    public interface IApplicationRepository : IRepository<Application, Guid>
    {
        Task<Application?> FindByName(string name, params string[] embeds);
    }
}
