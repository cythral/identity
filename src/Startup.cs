using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Brighid.Identity
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            var privKey = File.ReadAllText("/keys/privkey.pem")
            .Replace("-----BEGIN PRIVATE KEY-----\n", "")
            .Replace("\n-----END PRIVATE KEY-----\n", "");
            var privKeyBytes = Convert.FromBase64String(privKey);

#pragma warning disable CA2000 // This key will be needed throughout the entire app lifetime.
            var keyParams = RSA.Create();
            keyParams.ImportPkcs8PrivateKey(privKeyBytes, out _);
#pragma warning restore CA2000

            var key = new RsaSecurityKey(keyParams);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
            var jwtHeader = new JwtHeader(credentials);

            services.AddSingleton(jwtHeader);
            services.AddSingleton<JwtSecurityTokenHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".pem"] = "application/x-pem-file";

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider("/content"),
                ContentTypeProvider = provider
            });

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
