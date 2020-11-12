using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity.Applications
{
    public class Application : INormalizeable<DatabaseContext>
    {
        /// <summary>
        /// Gets the ID number for this application.
        /// </summary>
        /// <value>A unique id number.</value>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; init; }

        /// <summary>
        /// Gets the unique name for this application.
        /// </summary>
        /// <value>A globally unique identifier for the application.</value>
        public string Name { get; init; } = "";

        /// <summary>
        /// Gets or sets a description of the application.
        /// </summary>
        /// <value>A description of what the application is/does.</value>
        public string Description { get; set; } = "";

        /// <summary>
        /// Gets or sets the application's serial
        /// </summary>
        /// <value>Whenever this number changes, the client secret is regenerated.</value>
        public ulong Serial { get; set; }

        /// <summary>
        /// Gets the date this application was created.
        /// </summary>
        /// <value>The date/time this application was created.</value>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; init; }

        /// <summary>
        /// Gets a collection of application roles that belong to this Application. This is the backing property for roles.
        /// </summary>
        /// <value>The role mappings this application is allowed to use.</value>
        private ICollection<ApplicationRole> ApplicationRoles { get; set; } = Array.Empty<ApplicationRole>();

        /// <summary>
        /// Gets the roles this application is allowed to use.
        /// </summary>
        /// <returns>The roles this application is allowed to use.</returns>
        [NotMapped]
        public IEnumerable<string> Roles
        {
            get => ApplicationRoles.Select(appRole => appRole.Role.Name);
            set => ApplicationRoles = value
                    .Select(role => new ApplicationRole(this, role))
                    .ToArray();
        }

        /// <summary>
        /// Normalizes data so the entity can be safely added/updated in the database.
        /// </summary>
        /// <remarks>
        /// TODO: Unit test this
        /// </remarks>
        /// <param name="databaseContext">The database context to use for interacting with the database.</param>
        /// <param name="cancellationToken">Cancellation token to use for cancelling the task.</param>
        public virtual async Task Normalize([NotNull] DatabaseContext databaseContext, CancellationToken cancellationToken = default)
        {
            var givenRoles = ApplicationRoles;
            ApplicationRoles = await NormalizeApplicationRoles(databaseContext, cancellationToken)
            .SelectAwait(async appRole =>
            {
                appRole.Role = await NormalizeRole(databaseContext, appRole.Role, cancellationToken).ConfigureAwait(false);
                return appRole;
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        }

        private async IAsyncEnumerable<ApplicationRole> NormalizeApplicationRoles([NotNull] DatabaseContext databaseContext, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var givenRole in ApplicationRoles)
            {
                var givenRoleName = givenRole.Role.Name;
                var appRoleQuery = from appRole in databaseContext.ApplicationRoles.AsQueryable()
                                   where appRole.ApplicationId == Id
                                   where appRole.Role.Name == givenRoleName
                                   select appRole;

                if (await appRoleQuery.AnyAsync(cancellationToken).ConfigureAwait(false))
                {
                    yield return await appRoleQuery.FirstAsync(cancellationToken).ConfigureAwait(false);
                    continue;
                }

                yield return new ApplicationRole
                {
                    Application = this,
                    Role = new Role { Name = givenRoleName }
                };
            }
        }

        private async Task<Role> NormalizeRole([NotNull] DatabaseContext databaseContext, Role role, CancellationToken cancellationToken = default)
        {
            var roleQuery = from existingRole in databaseContext.Roles.AsQueryable()
                            where existingRole.Name == role.Name
                            select existingRole;

            var roleExists = await roleQuery.AnyAsync(cancellationToken);
            return roleExists
                ? await roleQuery.FirstAsync(cancellationToken)
                : role;
        }
    }
}
