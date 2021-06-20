using Brighid.Identity.LoginProviders;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LoginProvidersServiceCollectionExtensions
    {
        public static void ConfigureLoginProvidersServices(this IServiceCollection services)
        {
            services.AddScoped<ILoginProviderRepository, DefaultLoginProviderRepository>();
        }
    }
}
