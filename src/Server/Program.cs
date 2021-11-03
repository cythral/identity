using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace Brighid.Identity
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("Environment") ?? Environments.Development;

            return Host
            .CreateDefaultBuilder(args)
            .UseEnvironment(environment)
            .UseSerilog(dispose: true)
            .UseDefaultServiceProvider(options =>
            {
#pragma warning disable IDE0078 // Use Pattern Matching

                var isDevOrLocal = environment == Environments.Local || environment == Environments.Development;
                options.ValidateScopes = isDevOrLocal;
                options.ValidateOnBuild = isDevOrLocal;

#pragma warning restore IDE0078
            })
            .ConfigureAppConfiguration(configure =>
            {
                configure.AddEnvironmentVariables();
                configure.AddInMemoryCollection(new Dictionary<string, string>
                {
                    [WebHostDefaults.StaticWebAssetsKey] = "StaticWebAssets.xml",
                });
            })
            .ConfigureWebHostDefaults(builder =>
            {
                builder.UseStartup<Startup>();
                builder.ConfigureKestrel((context, options) =>
                {
                    var appConfig = context.Configuration.GetSection("App").Get<AppConfig>() ?? new AppConfig();
                    options.ListenAnyIP(appConfig.Port, listenOptions =>
                    {
                        listenOptions.Protocols = appConfig.Protocols;
                    });
                });
            });
        }
    }
}
