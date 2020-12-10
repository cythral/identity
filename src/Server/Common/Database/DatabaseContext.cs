using System;

using Brighid.Identity.Applications;
using Brighid.Identity.Users;
using Brighid.Identity.Roles;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Brighid.Identity
{
    public class DatabaseContext
        : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
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
            .Entity<UserRole>()
            .HasKey(userRole => new { userRole.UserId, userRole.RoleId });

            builder
            .Entity<User>()
            .HasMany<UserRole>("UserRoles")
            .WithOne(userRole => userRole.User);

            builder
            .Entity<Role>()
            .HasIndex(role => role.Name)
            .IsUnique();

            builder
            .Entity<UserLoginAttribute>()
            .HasKey(attr => new { attr.Key, attr.LoginId });
#pragma warning restore IDE0050, CA1507
        }

        public virtual DbSet<Application> Applications { get; init; }

        public virtual DbSet<ApplicationRole> ApplicationRoles { get; init; }
    }
}
