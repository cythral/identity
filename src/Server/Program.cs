using System.Net;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
            .ConfigureWebHostDefaults(builder =>
            {
                builder.UseStartup<Startup>();
                builder.ConfigureKestrel((context, options) =>
                {
                    var appConfig = context.Configuration.GetSection("App").Get<AppConfig>();
                    options.Listen(IPAddress.Any, appConfig.Port, listenOptions =>
                    {
                        listenOptions.Protocols = appConfig.Protocols;
                    });
                });
            });
        }
    }
}
