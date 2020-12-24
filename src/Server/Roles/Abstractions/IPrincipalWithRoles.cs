using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Brighid.Identity.Roles
{
    public interface IPrincipalWithRoles<in TPrincipal, TPrincipalRoleJoin>
        where TPrincipalRoleJoin : IPrincipalRoleJoin<TPrincipal>
    {
        string Name { get; }

        ICollection<TPrincipalRoleJoin> Roles { get; set; }

        TPrincipalRoleJoin? GetRoleJoin(string roleName)
        {
            var normalizedName = roleName.ToUpper(CultureInfo.InvariantCulture);
            var query = from role in Roles
                        where role.Role.NormalizedName == normalizedName
                        select role;

            return query.FirstOrDefault();
        }
    }
}
