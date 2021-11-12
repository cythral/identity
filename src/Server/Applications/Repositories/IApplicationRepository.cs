using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Applications
{
    public interface IApplicationRepository : IRepository<Application, Guid>
    {
        Task<Application?> FindByName(string name, params string[] embeds);

        Task<IEnumerable<Role>?> FindRolesById(Guid applicationId, params string[] embeds);
    }
}
