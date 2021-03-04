using System;

using Brighid.Identity.Applications;
using Brighid.Identity.LoginProviders;
using Brighid.Identity.Roles;
using Brighid.Identity.Users;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity
{
    public class DatabaseContext
        : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
    {
        public DatabaseContext(DbContextOptions options) : base(options) { }
        internal DatabaseContext() { }

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
            .Entity<UserRole>()
            .HasKey(userRole => new { userRole.UserId, userRole.RoleId });

            builder
            .Entity<Role>()
            .HasIndex(role => role.Name)
            .IsUnique();

            builder
            .Entity<UserLogin>()
            .HasAlternateKey(login => new { login.UserId, login.LoginProvider });

            builder
            .Entity<UserLogin>()
            .HasAlternateKey(login => new { login.LoginProvider, login.ProviderKey });

            builder
            .Entity<UserLoginAttribute>()
            .HasKey(attr => new { attr.Key, attr.LoginId });
#pragma warning restore IDE0050, CA1507
        }

        public virtual DbSet<Application> Applications { get; init; }

        public virtual DbSet<ApplicationRole> ApplicationRoles { get; init; }

        public virtual DbSet<LoginProvider> LoginProviders { get; init; }

        public override DbSet<UserRole> UserRoles { get; set; }
    }
}
