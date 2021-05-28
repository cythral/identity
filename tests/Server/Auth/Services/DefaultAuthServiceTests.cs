using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Users;

using FluentAssertions;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

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

                await authUtils.Received().CreateClaimsIdentityForApplication(Is(id), Is(cancellationToken));
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

                authUtils.CreateClaimsIdentityForApplication(Any<Guid>(), Any<CancellationToken>()).Returns(claimsIdentity);
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

                authUtils.CreateClaimsIdentityForApplication(Any<Guid>(), Any<CancellationToken>()).Returns(claimsIdentity);
                authUtils.CreateAuthTicket(Any<ClaimsIdentity>(), Any<IEnumerable<string>>()).Returns(authenticationTicket);

                var result = await authService.ClientExchange(request, cancellationToken);

                result.Should().Be(authenticationTicket);
                authUtils.Received().CreateAuthTicket(Is(claimsIdentity), Is<IEnumerable<string>>(req =>
                    scopes.All(req.Contains) &&
                    req.Contains("openid")
                ));
            }
        }

        [Category("Unit")]
        public class PasswordExchange
        {
            [Test, Auto]
            public async Task ShouldThrowIfUserNotFound(
                string email,
                string password,
                Uri redirectUri,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                userManager.FindByEmailAsync(Any<string>()).Returns((User)null!);

                Func<Task> func = () => service.PasswordExchange(email, password, redirectUri, cancellationToken);

                await func.Should().ThrowAsync<InvalidCredentialsException>();
                await userManager.Received().FindByEmailAsync(Is(email));
            }

            [Test, Auto]
            public async Task ShouldThrowIfPasswordIsInvalid(
                string email,
                string password,
                Uri redirectUri,
                [Frozen] User user,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                userManager.CheckPasswordAsync(Any<User>(), Any<string>()).Returns(false);

                Func<Task> func = () => service.PasswordExchange(email, password, redirectUri, cancellationToken);

                await func.Should().ThrowAsync<InvalidCredentialsException>();
                await userManager.Received().CheckPasswordAsync(Is(user), Is(password));
            }

            [Test, Auto]
            public async Task ShouldCreateClaimsIdentityForUser(
                string email,
                string password,
                Uri redirectUri,
                [Frozen] User user,
                [Frozen, Substitute] UserManager<User> userManager,
                [Frozen, Substitute] IAuthUtils authUtils,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                userManager.CheckPasswordAsync(Any<User>(), Any<string>()).Returns(true);

                await service.PasswordExchange(email, password, redirectUri, cancellationToken);

                await authUtils.Received().CreateClaimsIdentityForUser(Is(user), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldCreateAndReturnAuthTicket(
                string email,
                string password,
                Uri redirectUri,
                [Frozen] ClaimsIdentity claimsIdentity,
                [Frozen] AuthenticationTicket ticket,
                [Frozen, Substitute] UserManager<User> userManager,
                [Frozen, Substitute] IAuthUtils authUtils,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                userManager.CheckPasswordAsync(Any<User>(), Any<string>()).Returns(true);

                var result = await service.PasswordExchange(email, password, redirectUri, cancellationToken);

                result.Should().Be(ticket);
                authUtils.Received().CreateAuthTicket(Is(claimsIdentity), Is<string[]>(scopes => scopes.Contains("openid")), Is(redirectUri), Is(IdentityConstants.ApplicationScheme));
            }

            [Test, Auto]
            public async Task AuthTicketShouldHaveAccessTokenJwt(
                string email,
                string password,
                string accessToken,
                Uri redirectUri,
                [Frozen, Substitute] UserManager<User> userManager,
                [Frozen, Substitute] IAuthUtils authUtils,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                userManager.CheckPasswordAsync(Any<User>(), Any<string>()).Returns(true);
                authUtils.GenerateAccessToken(Any<AuthenticationTicket>()).Returns(accessToken);

                var result = await service.PasswordExchange(email, password, redirectUri, cancellationToken);

                result.Properties.GetTokens().Should().Contain(token => token.Name == "jwt" && token.Value == accessToken);
            }
        }
    }
}
