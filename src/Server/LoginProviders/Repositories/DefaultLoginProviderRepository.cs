namespace Brighid.Identity.LoginProviders
{
    public class DefaultLoginProviderRepository : Repository<LoginProvider, string>, ILoginProviderRepository
    {
        public DefaultLoginProviderRepository(DatabaseContext context)
            : base(context)
        {
        }
    }
}
