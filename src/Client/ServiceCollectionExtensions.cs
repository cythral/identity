using System;
using System.Linq;

using Brighid.Identity.Client;

using Microsoft.Extensions.DependencyInjection.Extensions;

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
