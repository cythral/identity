using System;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;

namespace Brighid.Identity.Applications
{
    [ScopedService]
    public interface IApplicationRepository : IRepository<Application, Guid>
    {
    }
}
