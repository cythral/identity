using System.Collections.Generic;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Applications
{
    [ScopedService]
    public interface IApplicationRoleService
    {
        void UpdateApplicationRoles(Application application, ICollection<ApplicationRole> roles);
    }
}
