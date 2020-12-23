namespace Brighid.Identity.Roles
{
    /// <summary>
    /// Names of built-in roles
    /// </summary>
    public enum BuiltInRole
    {
        /// <summary>The Basic Role given to all new users.</summary>
        Basic,

        /// <summary>Users who can create/update their own applications.</summary>
        ApplicationManager,

        /// <summary>Users with all privileges.</summary>
        Administrator,
    }
}
