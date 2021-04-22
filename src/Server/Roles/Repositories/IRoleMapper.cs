using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Roles
{
    [ScopedService]
    public interface IRoleMapper : IRequestToEntityMapper<Role, Role>
    {
    }
}
