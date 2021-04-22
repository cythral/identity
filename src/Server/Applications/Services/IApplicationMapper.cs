using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Applications
{
    [ScopedService]
    public interface IApplicationMapper : IRequestToEntityMapper<ApplicationRequest, Application>
    {
    }
}
