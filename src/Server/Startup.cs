using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Amazon.KeyManagementService;
using Amazon.SimpleSystemsManagement;

using Brighid.Identity.Roles;
using Brighid.Identity.Sns;

using Flurl.Http;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

#pragma warning disable IDE0061

namespace Brighid.Identity
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
            DatabaseConfig = Configuration.GetSection("Database").Get<DatabaseConfig>() ?? new DatabaseConfig();
            NetworkConfig = Configuration.GetSection("Network").Get<NetworkConfig>() ?? new NetworkConfig();
            AppConfig = Configuration.GetSection("App").Get<AppConfig>() ?? new AppConfig();
        }

        public IWebHostEnvironment Environment { get; }

        public IConfiguration Configuration { get; }

        public DatabaseConfig DatabaseConfig { get; }

        public NetworkConfig NetworkConfig { get; }

        public AppConfig AppConfig { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services
            .AddRazorPages()
            .WithRazorPagesRoot("/Common/Pages");

            services.AddHealthChecks();
            services.AddDbContextPool<DatabaseContext>(ConfigureDatabaseOptions);
            services.AddSwaggerGen();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.Configure<EncryptionOptions>(Configuration.GetSection("EncryptionOptions"));
            services.ConfigureUsersServices();
            services.ConfigureRolesServices();
            services.ConfigureLoginProvidersServices();
            services.ConfigureAuthServices(options => Configuration.Bind("Auth", options));
            services.ConfigureApplicationsServices();

            services.AddSingleton<IAmazonKeyManagementService, AmazonKeyManagementServiceClient>();
            services.AddSingleton<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>();
            services.AddSingleton<IEncryptionService, DefaultEncryptionService>();

            services.AddSingleton<GenerateRandomString>(Utils.GenerateRandomString);
            services.AddSingleton<GetOpenIdConnectRequest>(Utils.GetOpenIdConnectRequest);

            var jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            jsonOptions.Converters.Add(new JsonStringEnumConverter());
            FlurlHttp.GlobalSettings.JsonSerializer = new Serializer(jsonOptions);
        }

        public void ConfigureDatabaseOptions(DbContextOptionsBuilder options)
        {
            var conn = $"Server={DatabaseConfig.Host};";
            conn += $"Database={DatabaseConfig.Name};";
            conn += $"User={DatabaseConfig.User};";
            conn += $"Password=\"{DatabaseConfig.Password}\";";
            conn += "GuidFormat=Binary16";

            options
            .UseMySql(
                conn,
                new MySqlServerVersion(new Version(5, 7, 0))
            );

            options.UseOpenIddict();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DatabaseContext context)
        {
            if (env.IsDevelopment())
            {
                context.Database.Migrate();
                app.UseWebAssemblyDebugging();
            }

            app.Use(async (context, next) =>
            {
                context.Items[Constants.RequestSource] = IdentityRequestSource.Direct;
                context.Request.Headers.TryGetValue("content-type", out var contentType);

                if (!contentType.Any() || contentType.Any(type => type.StartsWith("text/plain", StringComparison.OrdinalIgnoreCase)))
                {
                    context.Request.Headers["content-type"] = "application/json";
                }

                await next();
            });

            app.UseSwagger(options =>
            {
                options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    var paths = new OpenApiPaths();
                    foreach (var (key, value) in swaggerDoc.Paths)
                    {
                        if (key.StartsWith("/api"))
                        {
                            paths[key.Replace("/api", string.Empty)] = value;
                        }
                    }

                    swaggerDoc.Paths = paths;
                    swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/api/" } };
                });
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Brighid Identity Swagger");
            });

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMiddleware<SnsMiddleware>();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks("/healthcheck");
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });

            var provider = app.ApplicationServices;

            SeedRole(provider, nameof(BuiltInRole.Basic)).GetAwaiter().GetResult();
            SeedRole(provider, nameof(BuiltInRole.Impersonator)).GetAwaiter().GetResult();
        }

        public async Task SeedRole(IServiceProvider provider, string name)
        {
            using var scope = provider.CreateScope();
            var services = scope.ServiceProvider;
            var roleRepository = services.GetRequiredService<IRoleRepository>();
            var existingRole = await roleRepository.FindByName(name).ConfigureAwait(false);

            if (existingRole != null)
            {
                return;
            }

            var role = new Role { Name = name, NormalizedName = name.ToUpper() };
            await roleRepository.Add(role);
        }
    }
}
