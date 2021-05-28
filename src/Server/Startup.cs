using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Amazon.KeyManagementService;
using Amazon.SimpleSystemsManagement;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;
using AspNetCore.ServiceRegistration.Dynamic.Extensions;

using Brighid.Identity.Auth;
using Brighid.Identity.Roles;
using Brighid.Identity.Sns;
using Brighid.Identity.Users;

using Flurl.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using static OpenIddict.Abstractions.OpenIddictConstants;

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
            OpenIdConfig = Configuration.GetSection("OpenId").Get<OpenIdConfig>() ?? new OpenIdConfig();
            NetworkConfig = Configuration.GetSection("Network").Get<NetworkConfig>() ?? new NetworkConfig();
            AppConfig = Configuration.GetSection("App").Get<AppConfig>() ?? new AppConfig();
        }

        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public DatabaseConfig DatabaseConfig { get; }
        public OpenIdConfig OpenIdConfig { get; }
        public NetworkConfig NetworkConfig { get; }
        public AppConfig AppConfig { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                RequireExpirationTime = true,
                ValidateAudience = false,
                ValidateIssuer = true,
                RoleClaimType = Claims.Role,
                ValidIssuers = new string[] { $"https://{OpenIdConfig.DomainName}/", $"http://{OpenIdConfig.DomainName}/" },
                ClockSkew = TimeSpan.FromMinutes(5),
            };

            if (Directory.Exists(OpenIdConfig.CertificatesDirectory))
            {
                var issuerSigningKeys = new List<SecurityKey>();
                foreach (var file in Directory.GetFiles(OpenIdConfig.CertificatesDirectory))
                {
                    var certificate = new X509Certificate2(file);
                    issuerSigningKeys.Add(new X509SecurityKey(certificate));
                }

                tokenValidationParameters.IssuerSigningKeys = issuerSigningKeys;
            }
            else
            {
                tokenValidationParameters.IssuerSigningKeys = new[] { Utils.GenerateDevelopmentSecurityKey() };
            }

            services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.Configure<OpenIdConfig>(Configuration.GetSection("OpenId"));
            services.Configure<EncryptionOptions>(Configuration.GetSection("EncryptionOptions"));
            services.AddHealthChecks();
            services.AddSingleton(tokenValidationParameters);
            services.AddSingleton<IAmazonKeyManagementService, AmazonKeyManagementServiceClient>();
            services.AddSingleton<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>();
            services.AddServicesWithAttributeOfType<ScopedServiceAttribute>();
            services.AddServicesWithAttributeOfType<SingletonServiceAttribute>();
            services.AddDbContextPool<DatabaseContext>(ConfigureDatabaseOptions);
            services.AddHttpContextAccessor();
            services.AddSwaggerGen();

            services
            .AddRazorPages()
            .WithRazorPagesRoot("/Common/Pages");
            services.AddServerSideBlazor();

            services.AddSingleton<GenerateRandomString>(Utils.GenerateRandomString);
            services.AddSingleton<GetOpenIdConnectRequest>(Utils.GetOpenIdConnectRequest);
            services.AddIdentity<User, Role>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<DatabaseContext>()
            .AddDefaultTokenProviders();

            // Replace Default User Manager with an overridden one
            var oldUserManagerDescriptor = services.First(descriptor => descriptor.ServiceType == typeof(UserManager<User>));
            services.Remove(oldUserManagerDescriptor);
            services.AddScoped<UserManager<User>, DefaultUserManager>();

            services.AddTransient(provider =>
            {
                var user = provider.GetService<IHttpContextAccessor>()?.HttpContext?.User;
                return user ?? new GenericPrincipal(new GenericIdentity("Anonymous"), null);
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";
                options.Cookie.Domain = AppConfig.CookieDomain;
                options.Cookie.Name = AppConfig.CookieName;
                options.ReturnUrlParameter = AppConfig.RedirectUriParameter;
                options.TicketDataFormat = new AuthTicketFormat(tokenValidationParameters);
                options.Events = new IdentityCookieAuthenticationEvents();
            });

            services.AddOpenId(OpenIdConfig, tokenValidationParameters);
            services.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(IdentityPolicy.RestrictedToSelfByUserId), policy =>
                {
                    static object? parse(string? input) =>
                        input != null ? new Guid(input).ToString() : null;

                    policy.AddRequirements(new RestrictedToSelfPolicyRequirement("userId", Claims.Subject, parse));
                });
            });
            services.AddSingleton<IAuthorizationHandler, RestrictedToSelfPolicyHandler>();

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
            }

            app.Use(async (context, next) =>
            {
                context.Items[Constants.RequestSource] = IdentityRequestSource.Direct;
                context.Request.Headers.TryGetValue("content-type", out var contentType);

                if (!contentType.Any() || contentType.Any(type => type.StartsWith("text/plain", StringComparison.OrdinalIgnoreCase)))
                {
                    context.Request.Headers["content-type"] = "application/json";
                }

                context.Request.Headers.TryGetValue("x-forwarded-for", out var forwardedForAddressValues);
                if (forwardedForAddressValues.Any())
                {
                    context.Request.Scheme = "https";
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
                            paths[key.Replace("/api", "")] = value;
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

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMiddleware<SnsMiddleware>();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthcheck");
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            var provider = app.ApplicationServices;

            SeedBasicRole(provider).GetAwaiter().GetResult();
        }

        public async Task SeedBasicRole(IServiceProvider provider)
        {
            using var scope = provider.CreateScope();
            var services = scope.ServiceProvider;
            var roleRepository = services.GetRequiredService<IRoleRepository>();
            var existingRole = await roleRepository.FindByName("Basic").ConfigureAwait(false);

            if (existingRole != null)
            {
                return;
            }

            var role = new Role { Name = "Basic", NormalizedName = "BASIC" };
            await roleRepository.Add(role);
        }
    }
}
