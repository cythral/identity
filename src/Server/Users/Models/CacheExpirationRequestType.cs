namespace Brighid.Identity.Users
{
    /// <summary>
    /// Represents a type of cache expiration request.
    /// </summary>
    public enum CacheExpirationRequestType
    {
        /// <summary>
        /// Request to expire a user from external caches.
        /// </summary>
        User = 0,

        /// <summary>
        /// Request to expire a command from external caches.
        /// </summary>
        Command = 1,
    }
}
