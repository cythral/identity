using System;

#pragma warning disable CA1032

namespace Brighid.Identity.Users
{
    public class UserLoginNotFoundException : Exception
    {
        public UserLoginNotFoundException(string loginProvider, string providerKey)
            : base($"User Login for {loginProvider} with ID {providerKey} was not found.")
        {
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
        }

        public string LoginProvider { get; init; }

        public string ProviderKey { get; init; }
    }
}
