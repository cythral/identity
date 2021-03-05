using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Auth
{
    [ScopedService]
    public interface IAuthContext
    {
        string UserName { get; }
    }
}
