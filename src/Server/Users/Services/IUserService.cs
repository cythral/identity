using System;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity.Users
{
    public interface IUserService
    {
        Task<User> Create(string username, string password, string? role = null);

        Task<UserLogin> CreateLogin(Guid id, UserLogin loginInfo);

        /// <summary>
        /// Enables debug mode for the user with the given <paramref name="userId" />.
        /// </summary>
        /// <param name="userId">The ID of the user to enable debug mode for.</param>
        /// <param name="enabled">Value indicating whether or not debug mode should be enabled for a user.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task SetDebugMode(Guid userId, bool enabled, CancellationToken cancellationToken);
    }
}
