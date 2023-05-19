using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Amazon.KeyManagementService;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;

using Brighid.Identity.Applications;
using Brighid.Identity.Roles;
using Brighid.Identity.Users;

using Destructurama;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using Serilog;
using Serilog.Events;

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

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Destructure.UsingAttributes()
                .Enrich.FromLogContext()
                .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning)
                .Filter.ByExcluding("RequestPath = '/healthcheck' and (StatusCode = 200 or EventId.Name = 'ExecutingEndpoint' or EventId.Name = 'ExecutedEndpoint')")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:s}] {Message:lj} {Properties:j} {Exception}{NewLine}")
                .CreateLogger();
        }

        public IWebHostEnvironment Environment { get; }

        public IConfiguration Configuration { get; }

        public DatabaseConfig DatabaseConfig { get; }

        public NetworkConfig NetworkConfig { get; }

        public AppConfig AppConfig { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
            .AddControllers(options =>
            {
                options.Filters.Add(typeof(HttpStatusCodeExceptionFilter));
            })
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
            services.AddSwaggerGen(options =>
            {
                options.MapType<UserFlags>(() => new OpenApiSchema { Type = "integer", Format = "int64" });
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("2600:1f18:22e4:7b00::"), 56));
                options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("2600:1f18:2323:b900::"), 56));
                options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("2600:1f18:24a8:e000::"), 56));
            });

            services.Configure<AppConfig>(Configuration.GetSection("App"));
            services.Configure<EncryptionOptions>(Configuration.GetSection("EncryptionOptions"));
            services.ConfigureUsersServices();
            services.ConfigureRolesServices();
            services.ConfigureLoginProvidersServices();
            services.ConfigureAuthServices(options => Configuration.Bind("Auth", options));
            services.ConfigureApplicationsServices();

            services.AddSingleton<IAmazonKeyManagementService, AmazonKeyManagementServiceClient>();
            services.AddSingleton<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>();
            services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
            services.AddSingleton<IAmazonS3>(new AmazonS3Client(new AmazonS3Config { UseDualstackEndpoint = true }));
            services.AddSingleton<IEncryptionService, DefaultEncryptionService>();

            services.AddSingleton<GenerateRandomString>(Utils.GenerateRandomString);
            services.AddSingleton<GetOpenIdConnectRequest>(Utils.GetOpenIdConnectRequest);

            var jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            jsonOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public void ConfigureDatabaseOptions(DbContextOptionsBuilder options)
        {
            var factory = new DatabaseContextFactory(Configuration, ServerVersion.AutoDetect, options);
            factory.Configure(options => options.AddXRayInterceptor());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DatabaseContext context)
        {
            app.UseXRay("Identity");
            if (env.IsEnvironment(Environments.Local))
            {
                context.Database.Migrate();
            }

            if (env.IsEnvironment(Environments.Local) || env.IsEnvironment(Environments.Development))
            {
                app.UseWebAssemblyDebugging();
            }

            app.UseForwardedHeaders();
            app.Use(async (context, next) =>
            {
                context.Request.Headers.TryGetValue("content-type", out var contentType);

                if (!contentType.Any() || contentType.Any(type => type?.StartsWith("text/plain", StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    context.Request.Headers["content-type"] = "application/json";
                }

                await next();
            });

            app.UseSwagger(options =>
            {
                options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    Utils.NormalizeSwaggerForApiOnly(swaggerDoc);
                    swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/api/" } };
                });
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Brighid Identity Swagger");
            });

            var contentPaths = from path in AppConfig.ContentPaths.Split(',') where !string.IsNullOrEmpty(path) select path;
            var fileProviders = from path in contentPaths select new PhysicalFileProvider(path);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new CompositeFileProvider(fileProviders),
                ServeUnknownFileTypes = true,
            });

            app.UseBlazorFrameworkFiles();
            app.UseAuthentication();
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
            SeedRole(provider, nameof(BuiltInRole.ApplicationManager)).GetAwaiter().GetResult();

            if (AppConfig.WaitConditionHandle != null)
            {
                var seededApplication = SeedApplication(provider, "BrighidIdentityCloudFormation", new[] { nameof(BuiltInRole.ApplicationManager) }).GetAwaiter().GetResult();
                var response = JsonSerializer.Serialize(new
                {
                    Status = "SUCCESS",
                    UniqueId = "SeededApplication",
                    Data = seededApplication.Id + "\n" + seededApplication.EncryptedSecret,
                });

                new HttpClient()
                .PutAsync(AppConfig.WaitConditionHandle, new StringContent(response))
                .GetAwaiter()
                .GetResult();
            }
        }

        public async Task<Role?> SeedRole(IServiceProvider provider, string name)
        {
            using var scope = provider.CreateScope();
            var services = scope.ServiceProvider;
            var roleRepository = services.GetRequiredService<IRoleRepository>();
            var existingRole = await roleRepository.FindByName(name).ConfigureAwait(false);

            if (existingRole != null)
            {
                return existingRole;
            }

            var role = new Role { Name = name, NormalizedName = name.ToUpper() };
            await roleRepository.Add(role);
            return role;
        }

        public async Task<Application> SeedApplication(IServiceProvider provider, string name, IEnumerable<string> roles)
        {
            using var scope = provider.CreateScope();
            var services = scope.ServiceProvider;
            var appService = services.GetRequiredService<IApplicationService>();
            var appRepository = services.GetRequiredService<IApplicationRepository>();
            var appMapper = services.GetRequiredService<IApplicationMapper>();
            var existingApp = await appRepository.FindByName(name);

            if (existingApp != null)
            {
                return existingApp;
            }

            var appRequest = new ApplicationRequest { Name = name, Roles = roles.ToArray() };
            var mappedApp = await appMapper.MapRequestToEntity(appRequest);
            var result = await appService.Create(mappedApp);
            return result;
        }
    }
}
