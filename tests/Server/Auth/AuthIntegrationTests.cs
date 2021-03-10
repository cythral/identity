using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using PuppeteerSharp;

namespace Brighid.Identity.Auth
{
    [TestFixture, Category("Integration")]
    public class AuthIntegrationTests
    {
        private const string email = "test@tester.com";
        private const string password = "Password123!";

        [Test, Auto]
        public async Task LoginRedirect(
            AppFactory app,
            Browser browser
        )
        {
            var page = await browser.NewPageAsync();
            await page.GoToAsync(app.RootUri.ToString());

            // We should be redirected to login
            new Uri(page.Url).AbsolutePath.Should().Be("/login");
        }

        [Test, Auto]
        public async Task SignupThenLogin(
            AppFactory app,
            Browser browser
        )
        {
            var page = await browser.NewPageAsync();
            await page.GoToAsync($"{app.RootUri}signup");

            #region Signup
            {
                var emailField = await page.MainFrame.QuerySelectorAsync("#Email");
                await emailField.TypeAsync(email);

                var passwordField = await page.MainFrame.QuerySelectorAsync("#Password");
                await passwordField.TypeAsync(password);

                var confirmPasswordField = await page.MainFrame.QuerySelectorAsync("#ConfirmPassword");
                await confirmPasswordField.TypeAsync(password);

                var submitButton = await page.MainFrame.QuerySelectorAsync("input[type=submit]");
                await submitButton.ClickAsync();

                await page.MainFrame.WaitForNavigationAsync();
            }
            #endregion

            #region Ensure User Was Created
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var query = from user in dbContext.Users.AsQueryable() where user.Email == email select user;
                var exists = await query.AnyAsync();

                exists.Should().Be(true);
            }
            #endregion

            #region Logout (Delete Cookies)
            {
                var cookies = await page.GetCookiesAsync();
                await page.DeleteCookieAsync(cookies);
                await page.GoToAsync($"{app.RootUri}login", waitUntil: WaitUntilNavigation.DOMContentLoaded);
            }
            #endregion

            #region Login
            {
                var emailField = await page.MainFrame.QuerySelectorAsync("#Email");
                await emailField.TypeAsync(email);

                var passwordField = await page.MainFrame.QuerySelectorAsync("#Password");
                await passwordField.TypeAsync(password);

                var submitButton = await page.MainFrame.QuerySelectorAsync("input[type=submit]");
                await submitButton.ClickAsync();

                await page.MainFrame.WaitForNavigationAsync();

                var url = new Uri(page.Url);
                url.AbsolutePath.Should().Be("/");
            }
            #endregion
        }
    }
}
