using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace Brighid.Identity
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
            .CreateDefaultBuilder(args)
            .UseSerilog(dispose: true)
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
