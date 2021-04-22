using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Applications
{
    [ScopedService]
    public interface IApplicationRepository : IRepository<Application, Guid>
    {
        Task<Application?> FindByName(string name, params string[] embeds);
        Task<IEnumerable<Role>> FindRolesById(Guid applicationId, params string[] embeds);
    }
}
