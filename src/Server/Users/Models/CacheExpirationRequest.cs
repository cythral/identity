namespace Brighid.Identity.Users
{
    public readonly struct CacheExpirationRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheExpirationRequest" /> struct.
        /// </summary>
        /// <param name="id">ID of the entity being expired from external caches.</param>
        /// <param name="type">Type of cache expiration request.</param>
        public CacheExpirationRequest(
            string id,
            CacheExpirationRequestType type
        )
        {
            Id = id;
            Type = type;
        }

        /// <summary>
        /// Gets the ID of the entity being expired from external caches.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the type of cache expiration request.
        /// </summary>
        public CacheExpirationRequestType Type { get; init; }
    }
}
