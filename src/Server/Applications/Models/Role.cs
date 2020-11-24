using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brighid.Identity.Applications
{
    public class Role
    {
        /// <summary>
        /// Gets the ID number for this role.
        /// </summary>
        /// <value>A unique id number.</value>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; init; }

        /// <summary>
        /// Gets the name of the role.
        /// </summary>
        /// <value>The name of the role.</value>
        public string Name { get; init; } = "";

        /// <summary>
        /// Gets or sets the description of the role.
        /// </summary>
        /// <value>The description of the role.</value>
        public string Description { get; set; } = "";
    }
}
