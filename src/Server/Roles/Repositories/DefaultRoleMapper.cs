using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Applications
{
    public class DefaultRoleMapper : IRoleMapper
    {
        public Task<Role> MapRequestToEntity(Role request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(request);
        }
    }
}
