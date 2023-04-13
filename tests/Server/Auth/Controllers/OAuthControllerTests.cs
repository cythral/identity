using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using OpenIddict.Abstractions;

using static NSubstitute.Arg;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Brighid.Identity.Auth
{
    [Category("Unit")]
    public class OAuthControllerTests
    {
        [Category("Unit")]
        public class Exchange
        {
            [Test]
            [Auto]
            public async Task ShouldGetOpenIdConnectRequest(
                OpenIddictRequest request,
                [Frozen, Substitute] HttpContext httpContext,
                [Frozen, Substitute] GetOpenIdConnectRequest getOpenIdConnectRequest,
                [Target] OAuthController authController
            )
            {
                getOpenIdConnectRequest(Any<Controller>()).Returns(request);
                authController.ControllerContext = new ControllerContext { HttpContext = httpContext };
                request.GrantType = GrantTypes.ClientCredentials;

                var result = await authController.Exchange();

                getOpenIdConnectRequest.Received()(Is(authController));
            }

            [Test]
            [Auto]
            public async Task ShouldPerformClientExchange_AndSetPrincipal(
                OpenIddictRequest request,
                AuthenticationTicket ticket,
                [Frozen, Substitute] HttpContext httpContext,
                [Frozen, Substitute] IAuthService authService,
                [Frozen, Substitute] GetOpenIdConnectRequest getOpenIdConnectRequest,
                [Target] OAuthController authController
            )
            {
                getOpenIdConnectRequest(Any<Controller>()).Returns(request);
                authService.ClientExchange(Any<OpenIddictRequest>(), Any<CancellationToken>()).Returns(ticket);

                request.GrantType = GrantTypes.ClientCredentials;
                authController.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var requestAborted = httpContext.RequestAborted;
                var result = await authController.Exchange();
                var signinResult = result as SignInResult;

                signinResult.Should().NotBeNull();
                signinResult!.Principal.Should().Be(ticket.Principal);

                await authService.Received().ClientExchange(Is(request), Is(requestAborted));
            }

            [Test]
            [Auto]
            public async Task ShouldPerformClientExchange_AndSetProperties(
                OpenIddictRequest request,
                AuthenticationTicket ticket,
                [Frozen, Substitute] HttpContext httpContext,
                [Frozen, Substitute] IAuthService authService,
                [Frozen, Substitute] GetOpenIdConnectRequest getOpenIdConnectRequest,
                [Target] OAuthController authController
            )
            {
                getOpenIdConnectRequest(Any<Controller>()).Returns(request);
                authService.ClientExchange(Any<OpenIddictRequest>(), Any<CancellationToken>()).Returns(ticket);

                request.GrantType = GrantTypes.ClientCredentials;
                authController.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var requestAborted = httpContext.RequestAborted;
                var result = await authController.Exchange();
                var signinResult = result as SignInResult;

                signinResult.Should().NotBeNull();
                signinResult!.Properties.Should().Be(ticket.Properties);

                await authService.Received().ClientExchange(Is(request), Is(requestAborted));
            }

            [Test]
            [Auto]
            public async Task ShouldPerformClientExchange_AndSetScheme(
                OpenIddictRequest request,
                AuthenticationTicket ticket,
                [Frozen, Substitute] HttpContext httpContext,
                [Frozen, Substitute] IAuthService authService,
                [Frozen, Substitute] GetOpenIdConnectRequest getOpenIdConnectRequest,
                [Target] OAuthController authController
            )
            {
                getOpenIdConnectRequest(Any<Controller>()).Returns(request);
                authService.ClientExchange(Any<OpenIddictRequest>(), Any<CancellationToken>()).Returns(ticket);

                request.GrantType = GrantTypes.ClientCredentials;
                authController.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var requestAborted = httpContext.RequestAborted;
                var result = await authController.Exchange();
                var signinResult = result as SignInResult;

                signinResult.Should().NotBeNull();
                signinResult!.AuthenticationScheme.Should().Be(ticket.AuthenticationScheme);

                await authService.Received().ClientExchange(Is(request), Is(requestAborted));
            }

            [Test]
            [Auto]
            public async Task ShouldPerformImpersonateExchange_AndSetPrincipal(
                OpenIddictRequest request,
                AuthenticationTicket ticket,
                [Frozen, Substitute] HttpContext httpContext,
                [Frozen, Substitute] IAuthService authService,
                [Frozen, Substitute] GetOpenIdConnectRequest getOpenIdConnectRequest,
                [Target] OAuthController authController
            )
            {
                getOpenIdConnectRequest(Any<Controller>()).Returns(request);
                authService.ImpersonateExchange(Any<OpenIddictRequest>(), Any<CancellationToken>()).Returns(ticket);

                request.GrantType = Constants.GrantTypes.Impersonate;
                authController.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var requestAborted = httpContext.RequestAborted;
                var result = await authController.Exchange();
                var signinResult = result as SignInResult;

                signinResult.Should().NotBeNull();
                signinResult!.Principal.Should().Be(ticket.Principal);

                await authService.Received().ImpersonateExchange(Is(request), Is(requestAborted));
            }

            [Test]
            [Auto]
            public async Task ShouldPerformImpersonateExchange_AndSetProperties(
                OpenIddictRequest request,
                AuthenticationTicket ticket,
                [Frozen, Substitute] HttpContext httpContext,
                [Frozen, Substitute] IAuthService authService,
                [Frozen, Substitute] GetOpenIdConnectRequest getOpenIdConnectRequest,
                [Target] OAuthController authController
            )
            {
                getOpenIdConnectRequest(Any<Controller>()).Returns(request);
                authService.ImpersonateExchange(Any<OpenIddictRequest>(), Any<CancellationToken>()).Returns(ticket);

                request.GrantType = Constants.GrantTypes.Impersonate;
                authController.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var requestAborted = httpContext.RequestAborted;
                var result = await authController.Exchange();
                var signinResult = result as SignInResult;

                signinResult.Should().NotBeNull();
                signinResult!.Properties.Should().Be(ticket.Properties);

                await authService.Received().ImpersonateExchange(Is(request), Is(requestAborted));
            }

            [Test]
            [Auto]
            public async Task ShouldPerformImpersonateExchange_AndSetScheme(
                OpenIddictRequest request,
                AuthenticationTicket ticket,
                [Frozen, Substitute] HttpContext httpContext,
                [Frozen, Substitute] IAuthService authService,
                [Frozen, Substitute] GetOpenIdConnectRequest getOpenIdConnectRequest,
                [Target] OAuthController authController
            )
            {
                getOpenIdConnectRequest(Any<Controller>()).Returns(request);
                authService.ImpersonateExchange(Any<OpenIddictRequest>(), Any<CancellationToken>()).Returns(ticket);

                request.GrantType = Constants.GrantTypes.Impersonate;
                authController.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var requestAborted = httpContext.RequestAborted;
                var result = await authController.Exchange();
                var signinResult = result as SignInResult;

                signinResult.Should().NotBeNull();
                signinResult!.AuthenticationScheme.Should().Be(ticket.AuthenticationScheme);

                await authService.Received().ImpersonateExchange(Is(request), Is(requestAborted));
            }

            [Test]
            [Auto]
            public async Task ShouldThrowForUnknownGrantType(
                OpenIddictRequest request,
                AuthenticationTicket ticket,
                [Frozen, Substitute] IAuthService authService,
                [Frozen, Substitute] GetOpenIdConnectRequest getOpenIdConnectRequest,
                [Target] OAuthController authController
            )
            {
                getOpenIdConnectRequest(Any<Controller>()).Returns(request);
                authService.ClientExchange(Any<OpenIddictRequest>(), Any<CancellationToken>()).Returns(ticket);

                request.GrantType = "unknown";

                Func<Task> func = authController.Exchange;
                await func.Should().ThrowAsync<InvalidOperationException>();

                await authService.DidNotReceive().ClientExchange(Any<OpenIddictRequest>(), Any<CancellationToken>());
            }
        }
    }
}
