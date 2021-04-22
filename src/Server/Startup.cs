using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Amazon.KeyManagementService;

using AspNetCore.ServiceRegistration.Dynamic.Attributes;
using AspNetCore.ServiceRegistration.Dynamic.Extensions;

using Brighid.Identity.Auth;
using Brighid.Identity.Roles;
using Brighid.Identity.Sns;
using Brighid.Identity.Users;

using Flurl.Http;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        }

        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public DatabaseConfig DatabaseConfig { get; }
        public OpenIdConfig OpenIdConfig { get; }

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
            services.AddSingleton<IAmazonKeyManagementService, AmazonKeyManagementServiceClient>();
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
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = 403;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                        return Task.FromResult(0);
                    },
                    OnRedirectToLogin = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = 401;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                        return Task.FromResult(0);
                    }
                };
            });

            services.AddOpenId(OpenIdConfig);
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

                if (!env.IsDevelopment() && (context.Connection.RemoteIpAddress == null || !IPAddress.IsLoopback(context.Connection.RemoteIpAddress)))
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
