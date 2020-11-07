
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
#pragma warning disable IDE0050
            builder
            .Entity<ApplicationRole>()
            .HasKey(appRole => new { appRole.ApplicationName, appRole.RoleName });
#pragma warning restore IDE0050
        }

        public DbSet<User> Users { get; init; }

        public DbSet<Application> Applications { get; init; }

        public DbSet<Role> Roles { get; init; }

        public DbSet<ApplicationRole> ApplicationRoles { get; init; }
    }
}
