using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using AspNet.Security.OpenIdConnect.Primitives;

using Brighid.Identity;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OpenIdServiceCollectionExtensions
    {
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
                options.EnableAuthorizationEndpoint(openIdOptions.AuthorizationEndpoint);
                options.EnableLogoutEndpoint(openIdOptions.LogoutEndpoint);
                options.EnableUserinfoEndpoint(openIdOptions.UserInfoEndpoint);
                options.EnableTokenEndpoint(openIdOptions.TokenEndpoint);
                options.UseJsonWebTokens();
                options.DisableHttpsRequirement();
                options.AllowClientCredentialsFlow();
                options.RegisterClaims(
                    OpenIdConnectConstants.Claims.Role,
                    OpenIdConnectConstants.Claims.Subject
                );

                if (!Directory.Exists(openIdOptions.CertificatesDirectory))
                {
                    options.AddDevelopmentSigningCertificate();
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
