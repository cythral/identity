using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Amazon.KeyManagementService;

using AspNet.Security.OpenIdConnect.Primitives;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;
using AspNetCore.ServiceRegistration.Dynamic.Extensions;
using AspNetCore.ServiceRegistration.Dynamic.Interfaces;

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

#pragma warning disable IDE0061

namespace Brighid.Identity
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
            DatabaseConfig = Configuration.GetSection("Database").Get<DatabaseConfig>();
        }

        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public DatabaseConfig DatabaseConfig { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.Configure<EncryptionOptions>(Configuration.GetSection("EncryptionOptions"));
            services.AddHealthChecks();
            services.AddScoped<IAmazonKeyManagementService, AmazonKeyManagementServiceClient>();
            services.AddServicesOfType<IScopedService>();
            services.AddServicesWithAttributeOfType<ScopedServiceAttribute>();
            services.AddDbContextPool<DatabaseContext>(ConfigureDatabaseOptions);

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

            var oldSignInManagerDescriptor = services.First(descriptor => descriptor.ServiceType == typeof(SignInManager<User>));
            services.Remove(oldSignInManagerDescriptor);
            services.AddScoped<SignInManager<User>, DefaultSignInManager>();

            services.AddTransient(provider =>
            {
                var user = provider.GetService<IHttpContextAccessor>()?.HttpContext?.User;
                return user ?? new GenericPrincipal(new GenericIdentity("Anonymous"), null);
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            services.AddOpenIddict()
            .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<DatabaseContext>())
            .AddServer(options =>
            {
                options.EnableAuthorizationEndpoint("/oauth2/authorize");
                options.EnableLogoutEndpoint("/oauth2/logout");
                options.EnableUserinfoEndpoint("/oauth2/userinfo");
                options.EnableTokenEndpoint("/oauth2/token");
                options.UseJsonWebTokens();
                options.DisableHttpsRequirement();
                options.AllowClientCredentialsFlow();
                options.RegisterClaims(
                    OpenIdConnectConstants.Claims.Role,
                    OpenIdConnectConstants.Claims.Subject
                );

                if (!Directory.Exists("/certs"))
                {
                    return;
                }

                foreach (var file in Directory.GetFiles("/certs"))
                {
                    var certificate = new X509Certificate2(file);
                    options.AddSigningCertificate(certificate);
                }
            })
            .AddValidation();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(IdentityPolicy.RestrictedToSelfByUserId), policy =>
                {
                    static object? parse(string? input) =>
                        input != null ? new Guid(input).ToString() : null;

                    policy.AddRequirements(new RestrictedToSelfPolicyRequirement("userId", OpenIdConnectConstants.Claims.Subject, parse));
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

            options.UseMySql(conn, new MySqlServerVersion(new Version(5, 7, 0)));
            options.UseOpenIddict();

            if (Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.LogTo(Console.WriteLine);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DatabaseContext context)
        {
            if (env.IsDevelopment())
            {
                context.Database.EnsureCreated();
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

                if (!env.IsDevelopment())
                {
                    context.Request.Scheme = "https";
                }

                await next();
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
            SeedUser(env, provider, "test@example.com", "Password123!").GetAwaiter().GetResult();
        }

        public async Task SeedBasicRole(IServiceProvider provider)
        {
            using var scope = provider.CreateScope();
            var services = scope.ServiceProvider;
            var roleManager = services.GetRequiredService<RoleManager<Role>>();
            var role = new Role { Name = "Basic" };

            if (await roleManager.RoleExistsAsync(role.Name).ConfigureAwait(false))
            {
                return;
            }

            var result = await roleManager.CreateAsync(role).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                throw new Exception("Could not seed database with the Basic role.");
            }
        }

        public async Task SeedUser(IWebHostEnvironment env, IServiceProvider provider, string username, string password)
        {
            if (!env.IsDevelopment())
            {
                return;
            }

            using var scope = provider.CreateScope();
            var services = scope.ServiceProvider;
            var roleManager = services.GetRequiredService<RoleManager<Role>>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var context = services.GetRequiredService<DatabaseContext>();
            var id = new Guid("D4759009EB67427ABF21272509A27F1A");
            var concurrencyStamp = Guid.NewGuid().ToString();
            var user = new User { Id = id, UserName = username, Email = username, ConcurrencyStamp = concurrencyStamp };

            if (await userManager.FindByIdAsync(id.ToString()) != null)
            {
                return;
            }

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new Exception("Could not seed database with test user.");
            }

            var role = await roleManager.FindByNameAsync("Basic");
            var userRole = new UserRole { User = user, Role = role };

            context.UserRoles.Add(userRole);
            await context.SaveChangesAsync();
        }
    }
}
