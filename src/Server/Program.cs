using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Amazon.XRay.Recorder.Handlers.AwsSdk;

using Brighid.Identity.Auth;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace Brighid.Identity
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            var host = CreateHostBuilder(args).Build();

            var certificateUpdater = host.Services.GetRequiredService<ICertificateUpdater>();
            await certificateUpdater.UpdateCertificates();

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("Environment") ?? Environments.Local;

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
                configure.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [WebHostDefaults.StaticWebAssetsKey] = "StaticWebAssets.xml",
                });
            })
            .ConfigureWebHostDefaults(builder =>
            {
                if (Directory.Exists("/wwwroot"))
                {
                    builder.UseContentRoot("/wwwroot");
                }

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
