using System.Threading.Tasks;

using NUnit.Framework;

using PuppeteerSharp;

[SetUpFixture]
public class BrowserSetup
{
    public static Browser? Browser { get; set; }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        if (Browser != null)
        {
            await Browser.CloseAsync();
        }
    }
}
