
namespace Brighid.Identity.Applications
{
    public class DefaultApplicationRepository : Repository<Application, string>, IApplicationRepository
    {
        public DefaultApplicationRepository(DatabaseContext context) : base(context) { }
    }
}
