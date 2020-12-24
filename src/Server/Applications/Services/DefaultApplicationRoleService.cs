using System;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationRoleService : PrincipalRoleService<Application, Guid, ApplicationRole, IApplicationRoleRepository>, IApplicationRoleService
    {

        public DefaultApplicationRoleService(
            IApplicationRoleRepository applicationRoleRepository,
            IRoleRepository roleRepository
        ) : base(applicationRoleRepository, roleRepository)
        {
        }

    }
}
