using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.AspNetCore.Authentication;

using NSubstitute;

using NUnit.Framework;

using OpenIddict.Abstractions;

using static NSubstitute.Arg;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Brighid.Identity.Auth
{
    [Category("Unit")]
    public class DefaultAuthServiceTests
    {
        [Category("Unit")]
        public class ClientExchange
        {
            [Test, Auto]
            public async Task ShouldThrowIfNotClientGrantType(
                OpenIddictRequest request,
                [Target] DefaultAuthService authService
            )
            {
                request.GrantType = GrantTypes.AuthorizationCode;

                Func<Task> func = async () => await authService.ClientExchange(request);
                await func.Should().ThrowAsync<InvalidOperationException>();
            }

            [Test, Auto]
            public async Task ShouldCreateClaimsIdentity(
                Guid id,
                OpenIddictRequest request,
                [Frozen] IAuthUtils authUtils,
                [Target] DefaultAuthService authService,
                CancellationToken cancellationToken
            )
            {
                request.ClientId = id.ToString();
                request.GrantType = GrantTypes.ClientCredentials;

                await authService.ClientExchange(request, cancellationToken);

                await authUtils.Received().CreateClaimsIdentity(Is(id), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldCreateAuthTicket(
                Guid id,
                OpenIddictRequest request,
                ClaimsIdentity claimsIdentity,
                AuthenticationTicket authenticationTicket,
                [Frozen] IAuthUtils authUtils,
                [Target] DefaultAuthService authService,
                CancellationToken cancellationToken
            )
            {
                var clientId = id.ToString();
                request.ClientId = clientId;
                request.GrantType = GrantTypes.ClientCredentials;

                authUtils.CreateClaimsIdentity(Any<Guid>(), Any<CancellationToken>()).Returns(claimsIdentity);
                authUtils.CreateAuthTicket(Any<ClaimsIdentity>(), Any<IEnumerable<string>>()).Returns(authenticationTicket);

                var result = await authService.ClientExchange(request, cancellationToken);

                result.Should().Be(authenticationTicket);
                authUtils.Received().CreateAuthTicket(Is(claimsIdentity), Is<IEnumerable<string>>(scopes =>
                    scopes.Contains("openid")
                ));
            }

            [Test, Auto]
            public async Task ShouldCreateAuthTicket_WithScope(
                Guid id,
                string[] scopes,
                OpenIddictRequest request,
                AuthenticationTicket authenticationTicket,
                ClaimsIdentity claimsIdentity,
                [Frozen] IAuthUtils authUtils,
                [Target] DefaultAuthService authService,
                CancellationToken cancellationToken
            )
            {
                var clientId = id.ToString();
                request.ClientId = clientId;
                request.GrantType = GrantTypes.ClientCredentials;
                request.Scope = string.Join(' ', scopes);

                authUtils.CreateClaimsIdentity(Any<Guid>(), Any<CancellationToken>()).Returns(claimsIdentity);
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
