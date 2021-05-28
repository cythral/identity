using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.Auth
{
    public class AccountControllerTests
    {
        [TestFixture, Category("Unit")]
        public class RedirectToLinkStartUrlTests
        {
            [Test, Auto]
            public async Task ShouldRedirectToStartUrlIfFound(
                string url,
                string provider,
                HttpContext httpContext,
                [Frozen, Substitute] ILinkStartUrlService service,
                [Target] AccountController controller
            )
            {
                service.GetLinkStartUrlForProvider(Any<string>(), Any<CancellationToken>()).Returns(url);
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var result = await controller.RedirectToLinkStartUrl(provider) as RedirectResult;
                result.Should().NotBeNull();
                result!.Url.Should().Be(url);

                await service.Received().GetLinkStartUrlForProvider(Is(provider), Is(httpContext.RequestAborted));
            }

            [Test, Auto]
            public async Task ShouldReturnNotFoundResultIfLinkNotFound(
                string provider,
                HttpContext httpContext,
                [Frozen, Substitute] ILinkStartUrlService service,
                [Target] AccountController controller
            )
            {
                service.GetLinkStartUrlForProvider(Any<string>(), Any<CancellationToken>()).Returns((string?)null);
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var result = await controller.RedirectToLinkStartUrl(provider);
                result.Should().BeOfType<NotFoundResult>();

                await service.Received().GetLinkStartUrlForProvider(Is(provider), Is(httpContext.RequestAborted));
            }
        }
    }
}
