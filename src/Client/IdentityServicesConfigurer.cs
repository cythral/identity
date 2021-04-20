using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Brighid.Identity.Client
{

#pragma warning disable CA1812

    internal class IdentityServicesConfigurer<TServiceType, TImplementation, TCredentials> : IIdentityServicesConfigurer
        where TServiceType : class
        where TImplementation : class, TServiceType
        where TCredentials : class, IClientCredentials
    {
        private readonly IServiceCollection services;

        public IdentityServicesConfigurer(IServiceCollection services)
        {
            this.services = services;
        }

        public void ConfigureServices(ConfigurationContext context)
        {
            var section = Configuration.GetSection(context.ConfigSectionName);
            services.Configure<TCredentials>(section);
            services.TryAddSingleton<TokenCache>();
            services.TryAddScoped<IdentityServerClient>();
            services.TryAddScoped<ClientCredentialsHandler<TCredentials>>();

            services
            .AddHttpClient<IdentityServerClient>(options => options.BaseAddress = context.IdentityServerUri);

            var baseUri = new Uri(context.BaseAddress);
            services
            .AddHttpClient<TServiceType, TImplementation>(typeof(TImplementation).FullName, options => options.BaseAddress = baseUri)
            .AddHttpMessageHandler<ClientCredentialsHandler<TCredentials>>();
        }

        private IConfiguration Configuration =>
            services.BuildServiceProvider().GetRequiredService<IConfiguration>();
    }
}
