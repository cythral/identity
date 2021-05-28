using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Users;

using FluentAssertions;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

#pragma warning disable SA1313

namespace Brighid.Identity.Auth
{
    [TestFixture]
    [Category("Unit")]
    public class LogoutControllerTests
    {
        [TestFixture]
        [Category("Unit")]
        public class LogoutTests
        {
            [Test]
            [Auto]
            public async Task ShouldSignOut(
                [Frozen, Substitute] SignInManager<User> signInManager,
                [Target] LogoutController controller
            )
            {
                await controller.Logout();
                await signInManager.Received().SignOutAsync();
            }

            [Test]
            [Auto]
            public async Task ShouldRedirectToLogin(
                [Frozen, Substitute] SignInManager<User> _,
                [Target] LogoutController controller
            )
            {
                var response = await controller.Logout();
                response.Should().BeOfType<LocalRedirectResult>();
                response.As<LocalRedirectResult>().Url.Should().Be("/login?redirect_uri=%2F");
            }
        }
    }
}
