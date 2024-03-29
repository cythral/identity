using System;

using Brighid.Identity.Applications;
using Brighid.Identity.LoginProviders;
using Brighid.Identity.Roles;
using Brighid.Identity.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

#pragma warning disable SA1402, SA1649

namespace Brighid.Identity
{
    public class RoleUser : IdentityUserRole<Guid>
    {
        public override Guid UserId { get; set; }

        public override Guid RoleId { get; set; }
    }

    public class DatabaseContext
        : IdentityDbContext<User, Role, Guid, UserClaim, RoleUser, UserLogin, RoleClaim, UserToken>
    {
        public DatabaseContext(DbContextOptions options)
            : base(options)
        {
        }

        internal DatabaseContext()
        {
        }

        public virtual DbSet<Application> Applications { get; init; }

        public virtual DbSet<LoginProvider> LoginProviders { get; init; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
#pragma warning disable IDE0050, CA1507
            builder
            .Entity<Application>()
            .HasIndex(app => app.Name)
            .IsUnique();

            builder
            .Entity<RoleUser>()
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
    }
}
