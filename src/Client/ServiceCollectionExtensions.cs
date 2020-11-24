using System;

using Brighid.Identity.Client;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void UseBrighidIdentity<TServiceType, TImplementation>(this IServiceCollection serviceCollection, string baseAddress)
            where TServiceType : class
            where TImplementation : class, TServiceType
        {
            serviceCollection.TryAddSingleton<TokenCache>();
            serviceCollection.TryAddScoped<IdentityServerClient>();
            serviceCollection.TryAddScoped<ClientCredentialsHandler>();

            serviceCollection
            .AddHttpClient<IdentityServerClient>(options => options.BaseAddress = new Uri("https://identity.brigh.id/"));

            serviceCollection
            .AddHttpClient<TServiceType, TImplementation>(typeof(TImplementation).FullName, options => options.BaseAddress = new Uri(baseAddress))
            .AddHttpMessageHandler<ClientCredentialsHandler>();
        }
    }
}
