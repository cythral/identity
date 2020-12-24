using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Roles;

namespace Brighid.Identity.Applications
{
    [JsonConverter(typeof(ApplicationRoleConverter))]
    public class ApplicationRole : IPrincipalRoleJoin<Application>
    {
        public ApplicationRole() { }

        public ApplicationRole(Application application, string roleName)
        {
            Application = application;
            ApplicationId = application.Id;
            Role = new Role { Name = roleName };
        }

        public Guid ApplicationId { get; set; }

        public Guid RoleId { get; set; }

        [ForeignKey("ApplicationId")]
        public Application Application { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }

        [NotMapped]
        Application IPrincipalRoleJoin<Application>.Principal
        {
            get => Application;
            set => Application = value;
        }
    }
}
