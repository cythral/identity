using System;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Applications
{
    [ScopedService]
    public interface IApplicationRoleService : IPrincipalRoleService<Application, Guid, ApplicationRole, IApplicationRoleRepository>
    {
    }
}
