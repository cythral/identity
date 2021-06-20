using Brighid.Identity.Users;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UsersServiceCollectionExtensions
    {
        public static void ConfigureUsersServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, DefaultUserService>();
            services.AddScoped<IUserRepository, DefaultUserRepository>();
            services.AddScoped<IUserLoginRepository, DefaultUserLoginRepository>();
        }
    }
}
