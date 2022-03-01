using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Brighid.Identity
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        private readonly DatabaseConfig options;
        private readonly Func<string, ServerVersion> detectVersion;
        private readonly DbContextOptionsBuilder dbOptionsBuilder;

        public DatabaseContextFactory()
            : this(
                new ConfigurationBuilder().AddEnvironmentVariables().Build(),
                (_) => new MySqlServerVersion(new Version(5, 7, 0)),
                new DbContextOptionsBuilder<DatabaseContext>()
            )
        {
        }

        public DatabaseContextFactory(
            IConfiguration configuration,
            Func<string, ServerVersion> detectVersion,
            DbContextOptionsBuilder dbOptionsBuilder
        )
        {
            this.options = configuration.GetSection("Database").Get<DatabaseConfig>() ?? new DatabaseConfig();
            this.detectVersion = detectVersion;
            this.dbOptionsBuilder = dbOptionsBuilder;
        }

        public void Configure()
        {
            var conn = $"Server={options.Host};";
            conn += $"Database={options.Name};";
            conn += $"User={options.User};";
            conn += $"Password=\"{options.Password}\";";
            conn += "GuidFormat=Binary16;";
            conn += "UseCompression=true";

            var version = detectVersion(conn);
            dbOptionsBuilder.UseMySql(conn, version);
            dbOptionsBuilder.UseOpenIddict();
        }

        public DatabaseContext CreateDbContext(string[] args)
        {
            Configure();
            return new DatabaseContext(dbOptionsBuilder.Options);
        }
    }
}
