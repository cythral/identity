using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using Brighid.Identity;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OpenIdServiceCollectionExtensions
    {
        private static readonly HashSet<IServiceCollection> servicesWithDevelopmentCertificates = new();

        public static void AddOpenId(this IServiceCollection services, OpenIdConfig openIdOptions)
        {
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
                options.RegisterClaims(
                    Claims.Role,
                    Claims.Subject
                );

                options.UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough()
                        .EnableTokenEndpointPassthrough()
                        .DisableTransportSecurityRequirement();

                options.DisableAccessTokenEncryption();
                options.AddEphemeralEncryptionKey();

                if (!Directory.Exists(openIdOptions.CertificatesDirectory) && !servicesWithDevelopmentCertificates.Contains(services))
                {
                    options.AddDevelopmentSigningCertificate();
                    servicesWithDevelopmentCertificates.Add(services);
                    return;
                }

                foreach (var file in Directory.GetFiles(openIdOptions.CertificatesDirectory))
                {
                    var certificate = new X509Certificate2(file);
                    options.AddSigningCertificate(certificate);
                }
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
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
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = async (er) =>
                    {
                        await Task.CompletedTask;
                        Console.WriteLine(er.Exception.Message);
                    }
                };
                options.SaveToken = true;
                options.RefreshOnIssuerKeyNotFound = true;
                options.RequireHttpsMetadata = false;
                options.MetadataAddress = $"{openIdOptions.DomainName}.well-known/openid-configuration";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    ValidateAudience = false,
                    ValidateIssuer = true,
                    RoleClaimType = Claims.Role,
                    ValidIssuer = openIdOptions.DomainName,
                    ClockSkew = TimeSpan.FromMinutes(5),
                };
            });
        }
    }
}
