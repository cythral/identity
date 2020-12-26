using System;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Roles
{
    [ScopedService]
    public interface IRoleService : IEntityService<Role, Guid>
    {
    }
}
