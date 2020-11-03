using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string Name { get; init; }

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
    }
}
