using System.ComponentModel.DataAnnotations.Schema;

namespace Brighid.Identity.Applications
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

        public ulong ApplicationId { get; set; }

        public ulong RoleId { get; set; }

        [ForeignKey("ApplicationId")]
        public Application Application { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}
