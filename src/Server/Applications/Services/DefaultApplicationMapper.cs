using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationMapper : IApplicationMapper
    {
        private readonly IRoleRepository roleRepository;

        public DefaultApplicationMapper(
            IRoleRepository roleRepository
        )
        {
            this.roleRepository = roleRepository;
        }

        public async Task<Application> MapRequestToEntity(ApplicationRequest request, CancellationToken cancellationToken = default)
        {
            var application = new Application
            {
                Name = request.Name,
                Description = request.Description,
                Serial = request.Serial,
            };

            var roles = (await roleRepository.FindAllByName(request.Roles, cancellationToken)).ToList();
            var roleDict = roles.ToDictionary(role => role.Name!, role => role);
            var missingRoles = new List<RoleNotFoundException>();

            foreach (var role in request.Roles)
            {
                if (!roleDict.ContainsKey(role))
                {
                    missingRoles.Add(new RoleNotFoundException(role));
                }
            }

            if (missingRoles.Any())
            {
                throw new AggregateException(missingRoles);
            }

            application.Roles = roles;
            return application;
        }
    }
}
