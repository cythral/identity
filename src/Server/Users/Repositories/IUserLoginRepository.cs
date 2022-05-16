using System;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Identity.Users
{
    public interface IUserLoginRepository : IRepository<UserLogin, Guid>
    {
        /// <summary>
        /// Finds a user login based on provider name and key.
        /// </summary>
        /// <param name="loginProvider">The name of the login provider that the user login is for (ie discord/google/etc).</param>
        /// <param name="providerKey">The ID of the user within the login provider's system (ie the discord user id).</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task<UserLogin?> FindByProviderNameAndKey(string loginProvider, string providerKey, CancellationToken cancellationToken);
    }
}
