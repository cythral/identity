using System.ComponentModel.DataAnnotations.Schema;

namespace Brighid.Identity.Applications
{
    public class ApplicationRole
    {
        public string ApplicationName { get; init; } = "";

        public string RoleName { get; init; } = "";

        [ForeignKey("ApplicationName")]
        public Application Application { get; init; } = new Application();

        [ForeignKey("RoleName")]
        public Role Role { get; init; } = new Role();
    }
}
