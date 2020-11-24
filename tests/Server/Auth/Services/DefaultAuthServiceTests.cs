using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AspNet.Security.OpenIdConnect.Primitives;

using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.AspNetCore.Authentication;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.Auth
{
    public class DefaultAuthServiceTests
    {
        public class ClientExchange
        {
            [Test, Auto]
            public async Task ShouldThrowIfNotClientGrantType(
                OpenIdConnectRequest request,
                [Target] DefaultAuthService authService
            )
            {
                request.GrantType = OpenIdConnectConstants.GrantTypes.AuthorizationCode;

                Func<Task> func = async () => await authService.ClientExchange(request);
                await func.Should().ThrowAsync<InvalidOperationException>();
            }

            [Test, Auto]
            public async Task ShouldCreateClaimsIdentity(
                string name,
                OpenIdConnectRequest request,
                [Frozen] IAuthUtils authUtils,
                [Target] DefaultAuthService authService,
                CancellationToken cancellationToken
            )
            {
                var clientId = $"{name}@identity.brigh.id";
                request.ClientId = clientId;
                request.GrantType = OpenIdConnectConstants.GrantTypes.ClientCredentials;

                await authService.ClientExchange(request, cancellationToken);

                await authUtils.Received().CreateClaimsIdentity(Is(name), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldCreateAuthTicket(
                string name,
                OpenIdConnectRequest request,
                ClaimsIdentity claimsIdentity,
                AuthenticationTicket authenticationTicket,
                [Frozen] IAuthUtils authUtils,
                [Target] DefaultAuthService authService,
                CancellationToken cancellationToken
            )
            {
                var clientId = $"{name}@identity.brigh.id";
                request.ClientId = clientId;
                request.GrantType = OpenIdConnectConstants.GrantTypes.ClientCredentials;

                authUtils.CreateClaimsIdentity(Any<string>(), Any<CancellationToken>()).Returns(claimsIdentity);
                authUtils.CreateAuthTicket(Any<ClaimsIdentity>(), Any<IEnumerable<string>>()).Returns(authenticationTicket);

                var result = await authService.ClientExchange(request, cancellationToken);

                result.Should().Be(authenticationTicket);
                authUtils.Received().CreateAuthTicket(Is(claimsIdentity), Is<IEnumerable<string>>(scopes =>
                    scopes.Contains("openid")
                ));
            }

            [Test, Auto]
            public async Task ShouldCreateAuthTicket_WithScope(
                string name,
                string[] scopes,
                OpenIdConnectRequest request,
                AuthenticationTicket authenticationTicket,
                ClaimsIdentity claimsIdentity,
                [Frozen] IAuthUtils authUtils,
                [Target] DefaultAuthService authService,
                CancellationToken cancellationToken
            )
            {
                var clientId = $"{name}@identity.brigh.id";
                request.ClientId = clientId;
                request.GrantType = OpenIdConnectConstants.GrantTypes.ClientCredentials;
                request.Scope = string.Join(' ', scopes);

                authUtils.CreateClaimsIdentity(Any<string>(), Any<CancellationToken>()).Returns(claimsIdentity);
                authUtils.CreateAuthTicket(Any<ClaimsIdentity>(), Any<IEnumerable<string>>()).Returns(authenticationTicket);

                var result = await authService.ClientExchange(request, cancellationToken);

                result.Should().Be(authenticationTicket);
                authUtils.Received().CreateAuthTicket(Is(claimsIdentity), Is<IEnumerable<string>>(req =>
                    scopes.All(req.Contains) &&
                    req.Contains("openid")
                ));
            }
        }
    }
}
