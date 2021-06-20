using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;

using Brighid.Identity.Interface.Auth;
using Brighid.Identity.Interface.Roles;
using Brighid.Identity.Roles;

using Majorsoft.Blazor.Extensions.BrowserStorage;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Identity.Interface
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddBrowserStorage();
            builder.Services.AddSingleton<JwtSecurityTokenHandler>();
            builder.Services.AddSingleton<IRoleService, DefaultRoleService>();
            builder.Services.AddScoped<AuthenticationStateProvider, AuthContextProvider>();
            builder.Services.AddSingleton(new HttpClient(new HttpCookieHandler())
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
            });

            var host = builder.Build();
            await host.RunAsync();
        }
    }
}
