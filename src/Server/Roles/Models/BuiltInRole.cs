namespace Brighid.Identity.Roles
{
    /// <summary>
    /// Names of built-in roles.
    /// </summary>
    public enum BuiltInRole
    {
        /// <summary>The Basic Role given to all new users.</summary>
        Basic,

        /// <summary>Users who can create/update/delete their own applications.</summary>
        [DelegatingRole(nameof(Administrator))]
        ApplicationManager,

        /// <summary>Users who can create/update/delete roles.</summary>
        [DelegatingRole(nameof(Administrator))]
        RoleManager,

        /// <summary>Applications that have the ability to impersonate users.</summary>
        [DelegatingRole(nameof(Administrator))]
        Impersonator,

        /// <summary>Users with all privileges.</summary>
        [DelegatingRole(nameof(Administrator))]
        Administrator,
    }
}
