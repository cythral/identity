using System;
using System.IO;
using System.Linq;
using System.Text.Json;

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using PuppeteerSharp;

internal class AutoAttribute : AutoDataAttribute
{
    public AutoAttribute() : base(Create) { }

    public static IFixture Create()
    {
        var fixture = new Fixture();

        fixture.Register(() => Options.Create(new IdentityOptions
        {
            Stores = new StoreOptions
            {
                ProtectPersonalData = false,
            }
        }));
        fixture.Register(() =>
        {
            if (BrowserSetup.Browser == null)
            {
                while (BrowserSetup.Browser == null)
                {
                    try
                    {
                        var args = Environment.GetEnvironmentVariable("CI") != null
                            ? new[] { "--no-sandbox" }
                            : Array.Empty<string>();

                        BrowserSetup.Browser = Puppeteer.LaunchAsync(new LaunchOptions
                        {
                            Args = args
                        }).GetAwaiter().GetResult();
                    }
                    catch (FileNotFoundException)
                    {
                        var fetcher = new BrowserFetcher();
                        fetcher.DownloadAsync(BrowserFetcher.DefaultRevision).GetAwaiter().GetResult();
                    }
                }
            }

            return BrowserSetup.Browser;
        });
        fixture.Register(() => AppFactory.Create().GetAwaiter().GetResult());
        fixture.Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        fixture.Customizations.Add(new TypeOmitter<JsonElement>());
        fixture.Customizations.Insert(-1, new TargetRelay());
        fixture.Behaviors
        .OfType<ThrowingRecursionBehavior>()
        .ToList()
        .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

}
