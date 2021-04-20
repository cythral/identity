using System;

using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Identity.Client
{
    public class IdentityOptionsBuilder<TServiceType, TImplementation>
        where TServiceType : class
        where TImplementation : class, TServiceType
    {
        private readonly IServiceCollection services;
        private readonly ConfigurationContext context;
        private IIdentityServicesConfigurer? configurer;

        public IdentityOptionsBuilder(IServiceCollection services)
        {
            this.services = services;
            context = new ConfigurationContext();
        }

        public IdentityOptionsBuilder<TServiceType, TImplementation> WithIdentityServerUri(Uri identityServerUri)
        {
            context.IdentityServerUri = identityServerUri;
            return this;
        }

        public IdentityOptionsBuilder<TServiceType, TImplementation> WithBaseAddress(string baseAddress)
        {
            context.BaseAddress = baseAddress;
            return this;
        }

        public IdentityOptionsBuilder<TServiceType, TImplementation> WithCredentials<TCredentials>(string sectionName)
            where TCredentials : class, IClientCredentials
        {
            context.ConfigSectionName = sectionName;
            configurer = new IdentityServicesConfigurer<TServiceType, TImplementation, TCredentials>(services);
            return this;
        }

        internal void Build()
        {
            if (configurer == null)
            {
                throw new Exception("Configuration for Brighid Identity not setup.");
            }

            configurer.ConfigureServices(context);
        }
    }
}
