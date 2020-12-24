namespace Brighid.Identity.Roles
{
    public interface IPrincipalRoleJoin<TPrincipal>
    {
        Role Role { get; set; }

        TPrincipal Principal { get; set; }
    }
}
