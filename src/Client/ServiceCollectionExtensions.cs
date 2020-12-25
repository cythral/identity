using System;

using Brighid.Identity.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void UseBrighidIdentity<TServiceType, TImplementation>(this IServiceCollection serviceCollection, Action<IdentityOptionsBuilder<TServiceType, TImplementation>> configure)
            where TServiceType : class
            where TImplementation : class, TServiceType
        {
            var builder = new IdentityOptionsBuilder<TServiceType, TImplementation>(serviceCollection);
            configure(builder);
            builder.Build();
        }
    }
}
