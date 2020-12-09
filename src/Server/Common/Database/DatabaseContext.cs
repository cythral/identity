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
#pragma warning disable IDE0050, CA1507
            builder
            .Entity<Application>()
            .HasIndex(app => app.Name)
            .IsUnique();

            builder
            .Entity<ApplicationRole>()
            .HasKey(appRole => new { appRole.ApplicationId, appRole.RoleId });

            builder
            .Entity<Application>()
            .HasMany<ApplicationRole>("ApplicationRoles")
            .WithOne(appRole => appRole.Application);

            builder
            .Entity<Role>()
            .HasIndex(role => role.Name)
            .IsUnique();

            builder
            .Entity<UserLoginAttribute>()
            .HasKey(attr => new { attr.Key, attr.LoginId });
#pragma warning restore IDE0050, CA1507
        }

        public virtual DbSet<User> Users { get; init; }

        public virtual DbSet<Application> Applications { get; init; }

        public virtual DbSet<Role> Roles { get; init; }

        public virtual DbSet<ApplicationRole> ApplicationRoles { get; init; }
    }
}
