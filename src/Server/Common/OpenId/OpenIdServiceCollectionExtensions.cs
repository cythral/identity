using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using Brighid.Identity;

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
            .AddValidation();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
        }
    }
}
