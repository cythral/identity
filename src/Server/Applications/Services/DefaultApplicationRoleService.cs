using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationRoleService : IApplicationRoleService
    {
        private readonly IApplicationRoleRepository repository;

        public DefaultApplicationRoleService(
            IApplicationRoleRepository repository
        )
        {
            this.repository = repository;
        }

        /// <summary>
        /// Updates a change-tracked application's roles.
        /// </summary>
        /// <param name="application">The application to update roles for.</param>
        /// <param name="roles">The roles to set for the application.</param>
        public void UpdateApplicationRoles(Application application, ICollection<ApplicationRole> roles)
        {
            var updatedDict = roles.ToDictionary(
                appRole => appRole.Role.Name.ToUpper(CultureInfo.InvariantCulture),
                appRole => appRole
            );

            var existingDict = application.Roles.ToDictionary(
                appRole => appRole.Role.NormalizedName,
                appRole => appRole
            );

            foreach (var (name, role) in existingDict)
            {
                if (!updatedDict.ContainsKey(name))
                {
                    application.Roles.Remove(role);
                    repository.TrackAsDeleted(role);
                }

                updatedDict.Remove(name);
            }

            foreach (var (_, role) in updatedDict)
            {
                application.Roles.Add(role);
            }
        }
    }
}
