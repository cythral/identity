using System.Numerics;

namespace Brighid.Identity.Applications
{
    public class DefaultApplicationRepository : Repository<Application, ulong>, IApplicationRepository
    {
        public DefaultApplicationRepository(DatabaseContext context) : base(context) { }
    }
}
