using System;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity.Users
{
    public interface IUserCacheService
    {
        /// <summary>
        /// Notifies external caches to clear entries for the given user ID.
        /// </summary>
        /// <param name="userId">The user to clear from external caches.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task ClearExternalUserCache(Guid userId, CancellationToken cancellationToken);
    }
}
