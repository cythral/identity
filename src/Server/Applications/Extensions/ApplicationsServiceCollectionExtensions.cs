using Brighid.Identity.Applications;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationsServiceCollectionExtensions
    {
        public static void ConfigureApplicationsServices(this IServiceCollection services)
        {
            services.AddScoped<IApplicationService, DefaultApplicationService>();
            services.AddScoped<IApplicationMapper, DefaultApplicationMapper>();
            services.AddScoped<IApplicationRepository, DefaultApplicationRepository>();
        }
    }
}
