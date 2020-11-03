
using Brighid.Identity.Applications;
using Brighid.Identity.Users;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
        }

        public DbSet<User> Users { get; init; }

        public DbSet<Application> Applications { get; init; }
    }
}
