using System;
using System.Numerics;

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationRepository : Repository<Application, Guid>, IApplicationRepository
    {
        public DefaultApplicationRepository(DatabaseContext context) : base(context) { }
    }
}
