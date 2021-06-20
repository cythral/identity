using Brighid.Identity.Roles;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RolesServiceCollectionExtensions
    {
        public static void ConfigureRolesServices(this IServiceCollection services)
        {
            services.AddScoped<IRoleService, DefaultRoleService>();
            services.AddScoped<IRoleRepository, DefaultRoleRepository>();
        }
    }
}
