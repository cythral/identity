using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace Brighid.Identity.Applications
{
    public class Application
    {
        /// <summary>
        /// Gets the unique name for this application.
        /// </summary>
        /// <value>A globally unique identifier for the application.</value>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
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
        /// Gets the role mappings this application is allowed to use.
        /// </summary>
        /// <value>The role mappings this application is allowed to use.</value>
        [JsonIgnore]
        public ICollection<ApplicationRole> ApplicationRoles { get; private set; } = new HashSet<ApplicationRole>();

        /// <summary>
        /// Gets the roles this application is allowed to use.
        /// </summary>
        /// <returns>The roles this application is allowed to use.</returns>
        [NotMapped]
        public IEnumerable<Role> Roles
        {
            get => ApplicationRoles.Select(appRole => appRole.Role);
            set => ApplicationRoles = value
                .Select(role => new ApplicationRole { Application = this, Role = role })
                .ToList();
        }
    }
}
