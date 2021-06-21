using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

using Brighid.Identity;
using Brighid.Identity.Auth;
using Brighid.Identity.Roles;
using Brighid.Identity.Users;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using OpenIddict.Server;

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
            services.AddHttpContextAccessor();

            var tokenValidationParameters = GetTokenValidationParameters(authConfig);
            services.AddSingleton(tokenValidationParameters);
            services.AddAspNetCoreIdentity();
            services.AddOpenIdServer(authConfig, tokenValidationParameters);
            services.AddOpenIdAuth(authConfig, tokenValidationParameters);
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
        /// <param name="tokenValidationParameters">Parameters to use when validating tokens.</param>
        public static void AddOpenIdServer(this IServiceCollection services, AuthConfig authConfig, TokenValidationParameters tokenValidationParameters)
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

                foreach (var signingKey in tokenValidationParameters.IssuerSigningKeys)
                {
                    options.AddSigningKey(signingKey);
                }
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        }

        /// <summary>
        /// Adds authorization and authentication schemes to protect endpoints using OpenID/OAuth2 tokens.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="authConfig">Auth config to use.</param>
        /// <param name="tokenValidationParameters">Parameters to use when validating tokens.</param>
        public static void AddOpenIdAuth(this IServiceCollection services, AuthConfig authConfig, TokenValidationParameters tokenValidationParameters)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";
                options.Cookie.Domain = authConfig.CookieDomain;
                options.Cookie.Name = authConfig.CookieName;
                options.ReturnUrlParameter = authConfig.RedirectUriParameter;
                options.TicketDataFormat = new AuthTicketFormat(tokenValidationParameters);
                options.Events = new IdentityCookieAuthenticationEvents(authConfig);
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

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
                        ? JwtBearerDefaults.AuthenticationScheme
                        : IdentityConstants.ApplicationScheme;
                };
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RefreshOnIssuerKeyNotFound = true;
                options.RequireHttpsMetadata = false;
                options.BackchannelHttpHandler = new Http2AuthMessageHandler();
                options.MetadataAddress = $"http://localhost/.well-known/openid-configuration";
                options.TokenValidationParameters = tokenValidationParameters;
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

        private static TokenValidationParameters GetTokenValidationParameters(AuthConfig authConfig)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                RequireExpirationTime = true,
                ValidateAudience = false,
                ValidateIssuer = true,
                RoleClaimType = Claims.Role,
                ValidIssuers = new string[] { $"https://{authConfig.DomainName}/", $"http://{authConfig.DomainName}/" },
                ClockSkew = TimeSpan.FromMinutes(5),
            };

            if (Directory.Exists(authConfig.CertificatesDirectory))
            {
                var issuerSigningKeys = new List<SecurityKey>();
                foreach (var file in Directory.GetFiles(authConfig.CertificatesDirectory))
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

            return tokenValidationParameters;
        }
    }
}
