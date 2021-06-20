using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

using Brighid.Identity;
using Brighid.Identity.Auth;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using OpenIddict.Server;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthServiceCollectionExtensions
    {
        public static void ConfigureAuthServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, DefaultAuthService>();
            services.AddScoped<IAuthUtils, DefaultAuthUtils>();
            services.AddScoped<ILinkStartUrlService, SsmLinkStartUrlService>();
            services.AddScoped<AuthenticationStateProvider, AuthContextProvider>();
        }

        /// <summary>
        /// Adds the OpenIddict Server services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="openIdOptions">Options to use for OpenID.</param>
        /// <param name="tokenValidationParameters">Parameters to use when validating tokens.</param>
        public static void AddOpenIdServer(this IServiceCollection services, OpenIdConfig openIdOptions, TokenValidationParameters tokenValidationParameters)
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
                options.SetAuthorizationEndpointUris(openIdOptions.AuthorizationEndpoint);
                options.SetLogoutEndpointUris(openIdOptions.LogoutEndpoint);
                options.SetUserinfoEndpointUris(openIdOptions.UserInfoEndpoint);
                options.SetTokenEndpointUris(openIdOptions.TokenEndpoint);
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
        /// <param name="appConfig">Application-wide configuration items to use.</param>
        /// <param name="tokenValidationParameters">Parameters to use when validating tokens.</param>
        public static void AddOpenIdAuth(this IServiceCollection services, AppConfig appConfig, TokenValidationParameters tokenValidationParameters)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";
                options.Cookie.Domain = appConfig.CookieDomain;
                options.Cookie.Name = appConfig.CookieName;
                options.ReturnUrlParameter = appConfig.RedirectUriParameter;
                options.TicketDataFormat = new AuthTicketFormat(tokenValidationParameters);
                options.Events = new IdentityCookieAuthenticationEvents(appConfig);
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
    }
}
