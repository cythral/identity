using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Applications
{
    public class Application : IPrincipalWithRoles<Application, ApplicationRole>
    {
        /// <summary>
        /// Gets the ID number for this application.  This also serves as the ClientId.
        /// </summary>
        /// <value>A unique id number.</value>
        [Key]
        public virtual Guid Id { get; internal set; } = Guid.NewGuid();

        /// <summary>
        /// Gets the unique name for this application.
        /// </summary>
        /// <value>A globally unique identifier for the application.</value>
        public virtual string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets a description of the application.
        /// </summary>
        /// <value>A description of what the application is/does.</value>
        public virtual string Description { get; set; } = "";

        /// <summary>
        /// Gets or sets the application's serial.
        /// </summary>
        /// <remarks>
        /// Whenever this number changes, the client secret is regenerated.
        /// </remarks>
        /// <value>The application's serial.</value>
        public virtual ulong Serial { get; set; }

        /// <summary>
        /// Gets the date this application was created.
        /// </summary>
        /// <value>The date/time this application was created.</value>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual DateTimeOffset CreatedDate { get; private set; }

        /// <summary>
        /// Gets or sets the roles this application is allowed to use.
        /// </summary>
        /// <remarks>
        /// This gets serialized to/from a list of role names.
        /// </remarks>
        /// <value>The roles this application is allowed to use.</value>
        public virtual ICollection<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();

        /// <summary>
        /// Gets or sets the encrypted form of the application/client secret, which is a randomly-generated,
        /// long-lived credential along with the application/client id. 
        /// </summary>
        /// <value>The encrypted application/client secret.</value>
        public virtual string EncryptedSecret { get; set; } = "";

        /// <summary>
        /// Gets or sets the un-encrypted form of the application/client secret.
        /// </summary>
        /// <remarks>
        /// This is not stored in the database and will only appear in API requests
        /// that result in the regeneration of the secret.
        /// </remarks>
        /// <value>The un-encrypted application/client secret.</value>
        [NotMapped]
        public virtual string? Secret { get; set; } = null;
    }
}
