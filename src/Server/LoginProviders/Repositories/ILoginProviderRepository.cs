using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.LoginProviders
{
    [ScopedService]
    public interface ILoginProviderRepository : IRepository<LoginProvider, string>
    {
    }
}
