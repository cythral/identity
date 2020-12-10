using System;
using System.ComponentModel.DataAnnotations.Schema;

using Brighid.Identity.Applications;

namespace Brighid.Identity.Roles
{
    public class ApplicationRole
    {
        public ApplicationRole(Application application, string roleName)
        {
            Application = application;
            ApplicationId = application.Id;
            Role = new Role { Name = roleName };
        }

        public ApplicationRole()
        {
        }

        public Guid ApplicationId { get; set; }

        public Guid RoleId { get; set; }

        [ForeignKey("ApplicationId")]
        public Application Application { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}
