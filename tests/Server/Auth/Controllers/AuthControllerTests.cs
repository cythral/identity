using System;
using System.Threading;
using System.Threading.Tasks;

using AspNet.Security.OpenIdConnect.Primitives;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.Auth
{
    public class OAuthControllerTests
    {
        public class Exchange
        {
            [Test, Auto]
            public async Task ShouldGetOpenIdConnectRequest(
                OpenIdConnectRequest request,
                [Frozen, Substitute] HttpContext httpContext,
                [Frozen, Substitute] GetOpenIdConnectRequest getOpenIdConnectRequest,
                [Target] OAuthController authController
            )
            {
                getOpenIdConnectRequest(Any<Controller>()).Returns(request);
                authController.ControllerContext = new ControllerContext { HttpContext = httpContext };
                request.GrantType = OpenIdConnectConstants.GrantTypes.ClientCredentials;

                var result = await authController.Exchange();

                getOpenIdConnectRequest.Received()(Is(authController));
            }

            [Test, Auto]
            public async Task ShouldPerformClientExchange_AndSetPrincipal(
                OpenIdConnectRequest request,
                AuthenticationTicket ticket,
                [Frozen, Substitute] HttpContext httpContext,
                [Frozen, Substitute] IAuthService authService,
                [Frozen, Substitute] GetOpenIdConnectRequest getOpenIdConnectRequest,
                [Target] OAuthController authController
            )
            {
                getOpenIdConnectRequest(Any<Controller>()).Returns(request);
                authService.ClientExchange(Any<OpenIdConnectRequest>(), Any<CancellationToken>()).Returns(ticket);

                request.GrantType = OpenIdConnectConstants.GrantTypes.ClientCredentials;
                authController.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var requestAborted = httpContext.RequestAborted;
                var result = await authController.Exchange();
                var signinResult = result as SignInResult;

                signinResult.Should().NotBeNull();
                signinResult!.Principal.Should().Be(ticket.Principal);

                await authService.Received().ClientExchange(Is(request), Is(requestAborted));
            }

            [Test, Auto]
            public async Task ShouldPerformClientExchange_AndSetProperties(
                OpenIdConnectRequest request,
                AuthenticationTicket ticket,
                [Frozen, Substitute] HttpContext httpContext,
                [Frozen, Substitute] IAuthService authService,
                [Frozen, Substitute] GetOpenIdConnectRequest getOpenIdConnectRequest,
                [Target] OAuthController authController
            )
            {
                getOpenIdConnectRequest(Any<Controller>()).Returns(request);
                authService.ClientExchange(Any<OpenIdConnectRequest>(), Any<CancellationToken>()).Returns(ticket);

                request.GrantType = OpenIdConnectConstants.GrantTypes.ClientCredentials;
                authController.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var requestAborted = httpContext.RequestAborted;
                var result = await authController.Exchange();
                var signinResult = result as SignInResult;

                signinResult.Should().NotBeNull();
                signinResult!.Properties.Should().Be(ticket.Properties);

                await authService.Received().ClientExchange(Is(request), Is(requestAborted));
            }

            [Test, Auto]
            public async Task ShouldPerformClientExchange_AndSetScheme(
                OpenIdConnectRequest request,
                AuthenticationTicket ticket,
                [Frozen, Substitute] HttpContext httpContext,
                [Frozen, Substitute] IAuthService authService,
                [Frozen, Substitute] GetOpenIdConnectRequest getOpenIdConnectRequest,
                [Target] OAuthController authController
            )
            {
                getOpenIdConnectRequest(Any<Controller>()).Returns(request);
                authService.ClientExchange(Any<OpenIdConnectRequest>(), Any<CancellationToken>()).Returns(ticket);

                request.GrantType = OpenIdConnectConstants.GrantTypes.ClientCredentials;
                authController.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var requestAborted = httpContext.RequestAborted;
                var result = await authController.Exchange();
                var signinResult = result as SignInResult;

                signinResult.Should().NotBeNull();
                signinResult!.AuthenticationScheme.Should().Be(ticket.AuthenticationScheme);

                await authService.Received().ClientExchange(Is(request), Is(requestAborted));
            }

            [Test, Auto]
            public async Task ShouldThrowForUnknownGrantType(
                OpenIdConnectRequest request,
                AuthenticationTicket ticket,
                [Frozen, Substitute] IAuthService authService,
                [Frozen, Substitute] GetOpenIdConnectRequest getOpenIdConnectRequest,
                [Target] OAuthController authController
            )
            {
                getOpenIdConnectRequest(Any<Controller>()).Returns(request);
                authService.ClientExchange(Any<OpenIdConnectRequest>(), Any<CancellationToken>()).Returns(ticket);

                request.GrantType = "unknown";

                Func<Task> func = async () => await authController.Exchange();
                await func.Should().ThrowAsync<InvalidOperationException>();

                await authService.DidNotReceive().ClientExchange(Any<OpenIdConnectRequest>(), Any<CancellationToken>());
            }
        }
    }
}
