using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity.Users
{
    /// <summary>
    /// Service for interacting with and manipulating users within the Brighid Identity system.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <param name="password">The password the user uses to login.</param>
        /// <param name="role">An optional role to place the user in.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting user.</returns>
        Task<User> Create(string username, string password, string? role = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user login by the provider name and key.  This will throw an exception if the login is not found, or if the login is disabled.
        /// </summary>
        /// <param name="loginProvider">The name of the login provider that the user login is for (ie discord/google/etc).</param>
        /// <param name="providerKey">The ID of the user within the login provider's system (ie the discord user id).</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The requested user login.</returns>
        Task<User> GetByLoginProviderKey(string loginProvider, string providerKey, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new user login for a given user.
        /// </summary>
        /// <param name="id">The id of the user to create a login for.</param>
        /// <param name="loginInfo">Info about the user login to create.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting user login.</returns>
        Task<UserLogin> CreateLogin(Guid id, UserLogin loginInfo, CancellationToken cancellationToken);

        /// <summary>
        /// Disables a user login based on the login provider and provider key.
        /// </summary>
        /// <param name="principal">The user requesting the operation.</param>
        /// <param name="loginProvider">The name of the login provider that the user login is for (ie discord/google/etc).</param>
        /// <param name="providerKey">The ID of the user within the login provider's system (ie the discord user id).</param>
        /// <param name="enabled">Whether or not the login should be enabled.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task SetLoginStatus(ClaimsPrincipal principal, string loginProvider, string providerKey, bool enabled, CancellationToken cancellationToken);

        /// <summary>
        /// Enables debug mode for the user with the given <paramref name="userId" />.
        /// </summary>
        /// <param name="principal">The user requesting the operation.</param>
        /// <param name="userId">The ID of the user to enable debug mode for.</param>
        /// <param name="enabled">Value indicating whether or not debug mode should be enabled for a user.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task SetDebugMode(ClaimsPrincipal principal, Guid userId, bool enabled, CancellationToken cancellationToken);
    }
}
