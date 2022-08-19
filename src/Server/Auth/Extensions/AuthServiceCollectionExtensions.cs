using System;
using System.Linq;
using System.Security.Principal;

using Brighid.Identity;
using Brighid.Identity.Auth;
using Brighid.Identity.Roles;
using Brighid.Identity.Users;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

using OpenIddict.Server;
using OpenIddict.Validation.AspNetCore;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthServiceCollectionExtensions
    {
        public static void ConfigureAuthServices(this IServiceCollection services, Action<AuthConfig> configure)
        {
            var authConfig = new AuthConfig();
            configure(authConfig);

            services.Configure(configure);
            services.AddScoped<IAuthService, DefaultAuthService>();
            services.AddScoped<IAuthUtils, DefaultAuthUtils>();
            services.AddScoped<ILinkStartUrlService, SsmLinkStartUrlService>();
            services.AddScoped<AuthenticationStateProvider, AuthContextProvider>();
            services.AddSingleton<AuthTicketFormat>();
            services.AddSingleton<ICertificateUpdater, DefaultCertificateUpdater>();
            services.AddSingleton<ICertificateManager, OptionsCertificateManager>();
            services.AddSingleton<ICertificateConfigurationService, DefaultCertificateConfigurationService>();
            services.AddSingleton<ICertificateFetcher, DefaultCertificateFetcher>();
            services.AddSingleton<IHostedService, CertificateUpdateTimer>();
            services.AddHttpContextAccessor();

            services.AddAspNetCoreIdentity();
            services.AddOpenIdServer(authConfig);
            services.AddOpenIdAuth(authConfig);
            services.AddAuthorizationPolicies();
        }

        public static void AddAspNetCoreIdentity(this IServiceCollection services)
        {
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
        }

        /// <summary>
        /// Adds the OpenIddict Server services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="authConfig">Auth config to use.</param>
        public static void AddOpenIdServer(this IServiceCollection services, AuthConfig authConfig)
        {
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
            });

            services
            .AddOpenIddict()
            .AddCore(options => options
                .UseEntityFrameworkCore()
                .UseDbContext<DatabaseContext>()
            )
            .AddServer(options =>
            {
                options.SetAuthorizationEndpointUris(authConfig.AuthorizationEndpoint);
                options.SetLogoutEndpointUris(authConfig.LogoutEndpoint);
                options.SetUserinfoEndpointUris(authConfig.UserInfoEndpoint);
                options.SetTokenEndpointUris(authConfig.TokenEndpoint);
                options.AllowClientCredentialsFlow();
                options.AllowCustomFlow(Constants.GrantTypes.Impersonate);
                options.RegisterClaims(
                    Claims.Role,
                    Claims.Subject
                );
                options.RemoveEventHandler(OpenIddictServerHandlers.Exchange.ValidateClientIdParameter.Descriptor);
                options.AddEventHandler(ValidateClientIdParameter.Descriptor);
                options.AddEventHandler(ValidateAccessTokenParameter.Descriptor);

                options.UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough()
                        .EnableTokenEndpointPassthrough()
                        .DisableTransportSecurityRequirement();

                options.DisableAccessTokenEncryption();
                options.AddEphemeralEncryptionKey();
                options.AddEphemeralSigningKey();
                options.Configure(serverOptions =>
                {
                    ConfigureTokenValidationParameters(serverOptions.TokenValidationParameters, authConfig);
                });
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
                options.Configure(configure =>
                {
                    ConfigureTokenValidationParameters(configure.TokenValidationParameters, authConfig);
                });
            });
        }

        /// <summary>
        /// Adds authorization and authentication schemes to protect endpoints using OpenID/OAuth2 tokens.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="authConfig">Auth config to use.</param>
        public static void AddOpenIdAuth(this IServiceCollection services, AuthConfig authConfig)
        {
            services
            .AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
            .Configure<IServiceProvider>((options, serviceProvider) =>
            {
                options.LoginPath = "/login";
                options.Cookie.Domain = authConfig.CookieDomain;
                options.Cookie.Name = authConfig.CookieName;
                options.ReturnUrlParameter = authConfig.RedirectUriParameter;
                options.TicketDataFormat = serviceProvider.GetRequiredService<AuthTicketFormat>();
                options.Events = new IdentityCookieAuthenticationEvents(authConfig);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "smart";
                options.DefaultAuthenticateScheme = "smart";
                options.DefaultChallengeScheme = "smart";
            })
            .AddPolicyScheme("smart", "Authorization Bearer or OIDC", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    return authHeader?.StartsWith("Bearer ") == true
                        ? OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
                        : IdentityConstants.ApplicationScheme;
                };
            });
        }

        /// <summary>
        /// Adds authorization policies to the service collection.
        /// </summary>
        /// <param name="services">Service collection to add authorization policies to.</param>
        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(IdentityPolicy.RestrictedToSelfByUserId), policy =>
                {
                    static object? Parse(string? input)
                    {
                        return input != null ? new Guid(input).ToString() : null;
                    }

                    policy.AddRequirements(new RestrictedToSelfPolicyRequirement("userId", Claims.Subject, Parse));
                });
            });

            services.AddSingleton<IAuthorizationHandler, RestrictedToSelfPolicyHandler>();
        }

        private static void ConfigureTokenValidationParameters(TokenValidationParameters parameters, AuthConfig authConfig)
        {
            parameters.RequireSignedTokens = true;
            parameters.ValidateIssuerSigningKey = true;
            parameters.RequireExpirationTime = true;
            parameters.ValidateAudience = false;
            parameters.ValidateIssuer = true;
            parameters.ValidateLifetime = true;
            parameters.RoleClaimType = Claims.Role;
            parameters.ValidIssuer = null;
            parameters.ValidIssuers = new string[] { $"https://{authConfig.DomainName}/", $"http://{authConfig.DomainName}/" };
        }
    }
}
