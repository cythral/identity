using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

using Brighid.Identity;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#pragma warning disable IDE0022

public class AppFactory : WebApplicationFactory<Startup>
{
    private readonly string databaseServerAddress;

    private readonly IHost host;
    private readonly IPEndPoint endpoint;

    public AppFactory(string databaseServerAddress)
    {
        this.databaseServerAddress = databaseServerAddress;

        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            socket.Listen(1);
            endpoint = (IPEndPoint)socket.LocalEndPoint!;
        }

        var hostBuilder = CreateHostBuilder();
        host = hostBuilder.ConfigureWebHostDefaults(webHostBuilder =>
        {
            SetContentRoot(webHostBuilder);
            ConfigureWebHost(webHostBuilder);
        }).Build();

        RootUri = new Uri($"http://localhost:{endpoint.Port}");
    }

    public static string? ContainerId { get; private set; }

    public static string? ServerIp { get; private set; }

    public Uri RootUri { get; set; }

    public override IServiceProvider Services => host.Services;

    public static async Task<AppFactory> Create()
    {
        if (Environment.GetEnvironmentVariable("SKIP_INTEGRATION_SETUP") == "true")
        {
            return null!;
        }

        var serverIp = await MySqlContainer.GetMysqlServerAddress();
        var app = new AppFactory(serverIp);
        await app.Start();
        return app;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((options) =>
            options
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                [WebHostDefaults.StaticWebAssetsKey] = "Interface.StaticWebAssets.xml",
                ["Database:Host"] = databaseServerAddress,
                ["Database:Name"] = MySqlContainer.DbName,
                ["Database:User"] = MySqlContainer.DbUser,
                ["Database:Password"] = MySqlContainer.DbPassword,
                ["EncryptionOptions:KmsKeyId"] = "alias/SecretsKey",
                ["Auth:DomainName"] = $"localhost:{endpoint.Port}",
                ["App:Port"] = endpoint.Port.ToString(),
                ["App:Protocols"] = HttpProtocols.Http1.ToString(),
            })
        );

        builder.ConfigureTestServices(services =>
        {
            services
            .AddScoped<IItemRepository>(provider => null!)
            .AddScoped<IItemMapper>(provider => null!)
            .AddScoped<IItemService>(provider => null!)
            .AddMvc()
            .AddApplicationPart(typeof(AppFactory).Assembly)
            .AddControllersAsServices();

            services
            .AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
        });
    }

    private async Task Start()
    {
        await host.StartAsync();
    }

    private void SetContentRoot(IWebHostBuilder builder)
    {
        var contentRoot = GetContentRootFromAssembly();
        if (contentRoot != null)
        {
            builder.UseContentRoot(contentRoot);
        }
    }

    // https://github.com/dotnet/aspnetcore/blob/4900ad44cb953a0742f6e49bb0607f7a8876cf2e/src/Mvc/Mvc.Testing/src/WebApplicationFactory.cs#L203
    private string? GetContentRootFromAssembly()
    {
        var metadataAttributes = GetContentRootMetadataAttributes(
            typeof(Startup).Assembly.FullName!,
            typeof(Startup).Assembly.GetName().Name!);

        string? contentRoot = null;
        for (var i = 0; i < metadataAttributes.Length; i++)
        {
            var contentRootAttribute = metadataAttributes[i];
            var contentRootCandidate = Path.Combine(
                AppContext.BaseDirectory,
                contentRootAttribute.ContentRootPath);

            var contentRootMarker = Path.Combine(
                contentRootCandidate,
                Path.GetFileName(contentRootAttribute.ContentRootTest));

            if (File.Exists(contentRootMarker))
            {
                contentRoot = contentRootCandidate;
                break;
            }
        }

        return contentRoot;
    }

    // https://github.com/dotnet/aspnetcore/blob/4900ad44cb953a0742f6e49bb0607f7a8876cf2e/src/Mvc/Mvc.Testing/src/WebApplicationFactory.cs#L249
    private WebApplicationFactoryContentRootAttribute[] GetContentRootMetadataAttributes(
            string tEntryPointAssemblyFullName,
            string tEntryPointAssemblyName)
    {
        var testAssembly = GetTestAssemblies();
        var metadataAttributes = testAssembly
            .SelectMany(a => a.GetCustomAttributes<WebApplicationFactoryContentRootAttribute>())
            .Where(a => string.Equals(a.Key, tEntryPointAssemblyFullName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(a.Key, tEntryPointAssemblyName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(a => a.Priority)
            .ToArray();

        return metadataAttributes;
    }
}
