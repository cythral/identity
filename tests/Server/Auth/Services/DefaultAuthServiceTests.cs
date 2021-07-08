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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using NSubstitute;

using NUnit.Framework;

using OpenIddict.Abstractions;
using OpenIddict.Server;

using static NSubstitute.Arg;
using static OpenIddict.Abstractions.OpenIddictConstants;

using ValidateTokenRequestContext = OpenIddict.Server.OpenIddictServerEvents.ValidateTokenRequestContext;

namespace Brighid.Identity.Auth
{
    [Category("Unit")]
    public class DefaultAuthServiceTests
    {
        [Category("Unit")]
        public class ClientExchange
        {
            [Test]
            [Auto]
            public async Task ShouldThrowIfNotAcceptableGrantType(
                OpenIddictRequest request,
                [Target] DefaultAuthService authService
            )
            {
                request.GrantType = GrantTypes.AuthorizationCode;

                Func<Task> func = async () => await authService.ClientExchange(request);
                await func.Should().ThrowAsync<InvalidOperationException>();
            }

            [Test]
            [Auto]
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

            [Test]
            [Auto]
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

            [Test]
            [Auto]
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
        public class ImpersonateExchange
        {
            [Test]
            [Auto]
            public async Task ShouldThrowIfNotImpersonateGrantType(
                OpenIddictRequest request,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                request.GrantType = GrantTypes.AuthorizationCode;

                Func<Task> func = () => service.ImpersonateExchange(request, cancellationToken);

                await func.Should().ThrowAsync<InvalidOperationException>();
            }

            [Test]
            [Auto]
            public async Task ShouldNotThrowIfImpersonateGrantType(
                OpenIddictRequest request,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                request.GrantType = Constants.GrantTypes.Impersonate;

                Func<Task> func = () => service.ImpersonateExchange(request, cancellationToken);

                await func.Should().NotThrowAsync<InvalidOperationException>();
            }

            [Test]
            [Auto]
            public async Task ShouldThrowIfUserIdIsNull(
                OpenIddictRequest request,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                request.GrantType = Constants.GrantTypes.Impersonate;
                request["user_id"] = null;

                Func<Task> func = () => service.ImpersonateExchange(request, cancellationToken);

                await func.Should().ThrowAsync<MissingAuthParameterException>();
            }

            [Test]
            [Auto]
            public async Task ShouldFindUserById(
                Guid userId,
                OpenIddictRequest request,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                request.GrantType = Constants.GrantTypes.Impersonate;
                request["user_id"] = userId.ToString();

                await service.ImpersonateExchange(request, cancellationToken);

                await userManager.Received().FindByIdAsync(Is(userId.ToString()));
            }

            [Test]
            [Auto]
            public async Task ShouldCreateClaimsIdentityForUser(
                Guid userId,
                OpenIddictRequest request,
                [Frozen] User user,
                [Frozen, Substitute] IAuthUtils authUtils,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                request.GrantType = Constants.GrantTypes.Impersonate;
                request["user_id"] = userId.ToString();

                await service.ImpersonateExchange(request, cancellationToken);

                await authUtils.Received().CreateClaimsIdentityForUser(Is(user), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldCreateAndReturnAuthenticationTicket(
                Guid userId,
                OpenIddictRequest request,
                string[] audiences,
                [Frozen] AuthenticationTicket ticket,
                [Frozen] ClaimsIdentity claimsIdentity,
                [Frozen, Substitute] IAuthUtils authUtils,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                request.GrantType = Constants.GrantTypes.Impersonate;
                request["user_id"] = userId.ToString();
                request.Audiences = audiences;

                var result = await service.ImpersonateExchange(request, cancellationToken);

                result.Should().Be(ticket);
                authUtils.Received().CreateAuthTicket(
                    Is(claimsIdentity),
                    Is<string[]>(scopes => scopes.Contains("openid")),
                    resources: Is(audiences)
                );
            }
        }

        [Category("Unit")]
        public class PasswordExchange
        {
            [Test]
            [Auto]
            public async Task ShouldThrowIfUserNotFound(
                string email,
                string password,
                Uri redirectUri,
                HttpContext httpContext,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                userManager.FindByEmailAsync(Any<string>()).Returns((User)null!);

                Func<Task> func = () => service.PasswordExchange(email, password, redirectUri, httpContext, cancellationToken);

                await func.Should().ThrowAsync<InvalidCredentialsException>();
                await userManager.Received().FindByEmailAsync(Is(email));
            }

            [Test]
            [Auto]
            public async Task ShouldThrowIfPasswordIsInvalid(
                string email,
                string password,
                Uri redirectUri,
                HttpContext httpContext,
                [Frozen] User user,
                [Frozen, Substitute] UserManager<User> userManager,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                userManager.CheckPasswordAsync(Any<User>(), Any<string>()).Returns(false);

                Func<Task> func = () => service.PasswordExchange(email, password, redirectUri, httpContext, cancellationToken);

                await func.Should().ThrowAsync<InvalidCredentialsException>();
                await userManager.Received().CheckPasswordAsync(Is(user), Is(password));
            }

            [Test]
            [Auto]
            public async Task ShouldCreateClaimsIdentityForUser(
                string email,
                string password,
                Uri redirectUri,
                HttpContext httpContext,
                [Frozen] User user,
                [Frozen, Substitute] UserManager<User> userManager,
                [Frozen, Substitute] IAuthUtils authUtils,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                userManager.CheckPasswordAsync(Any<User>(), Any<string>()).Returns(true);

                await service.PasswordExchange(email, password, redirectUri, httpContext, cancellationToken);

                await authUtils.Received().CreateClaimsIdentityForUser(Is(user), Is(cancellationToken));
            }

            [Test]
            [Auto]
            public async Task ShouldCreateAndReturnAuthTicket(
                string email,
                string password,
                Uri redirectUri,
                HttpContext httpContext,
                [Frozen] ClaimsIdentity claimsIdentity,
                [Frozen] AuthenticationTicket ticket,
                [Frozen, Substitute] UserManager<User> userManager,
                [Frozen, Substitute] IAuthUtils authUtils,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                userManager.CheckPasswordAsync(Any<User>(), Any<string>()).Returns(true);

                var result = await service.PasswordExchange(email, password, redirectUri, httpContext, cancellationToken);

                result.Should().Be(ticket);
                authUtils.Received().CreateAuthTicket(Is(claimsIdentity), Is<string[]>(scopes => scopes.Contains("openid")), Is(redirectUri), Is(IdentityConstants.ApplicationScheme));
            }

            [Test]
            [Auto]
            public async Task AuthTicketShouldHaveAccessTokenJwt(
                string email,
                string password,
                string accessToken,
                Uri redirectUri,
                HttpContext httpContext,
                [Frozen, Substitute] UserManager<User> userManager,
                [Frozen, Substitute] IAuthUtils authUtils,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                userManager.CheckPasswordAsync(Any<User>(), Any<string>()).Returns(true);
                authUtils.GenerateAccessToken(Any<AuthenticationTicket>(), Any<string>()).Returns(accessToken);

                var result = await service.PasswordExchange(email, password, redirectUri, httpContext, cancellationToken);

                result.Properties.GetTokens().Should().Contain(token => token.Name == "access_token" && token.Value == accessToken);
            }

            [Test]
            [Auto]
            public async Task AuthTicketShouldHaveIdTokenJwt(
                string email,
                string password,
                string idToken,
                HttpContext httpContext,
                Uri redirectUri,
                [Frozen] User user,
                [Frozen] AuthenticationTicket ticket,
                [Frozen, Substitute] UserManager<User> userManager,
                [Frozen, Substitute] IAuthUtils authUtils,
                [Target] DefaultAuthService service,
                CancellationToken cancellationToken
            )
            {
                userManager.CheckPasswordAsync(Any<User>(), Any<string>()).Returns(true);
                authUtils.GenerateIdToken(Any<AuthenticationTicket>(), Any<User>(), Any<string>()).Returns(idToken);

                var result = await service.PasswordExchange(email, password, redirectUri, httpContext, cancellationToken);

                result.Properties.GetTokens().Should().Contain(token => token.Name == "id_token" && token.Value == idToken);

                authUtils.Received().GenerateIdToken(Is(ticket), Is(user), Is($"{httpContext.Request.Scheme}://{httpContext.Request.Host}/"));
            }
        }

        [Category("Unit")]
        public class ExtractPrincipalFromRequestContextTests
        {
            [Test]
            [Auto]
            public void ShouldValidateAndReturnPrincipalFromToken(
                [Target] DefaultAuthService service
            )
            {
                var token = "eyJhbGciOiAibm9uZSIsInR5cCI6ICJhdCtqd3QifQ.eyJpc3MiOiJodHRwOi8vdmFsaWQtaXNzdWVyLyIsImF1ZCI6Imh0dHA6Ly92YWxpZC1pc3N1ZXIvIiwibmFtZSI6InRlc3QtdXNlciIsImV4cCI6OTk5OTk5OTk5OX0=.";
                var transaction = new OpenIddictServerTransaction { Options = new() };
                var context = new ValidateTokenRequestContext(transaction)
                {
                    Issuer = new Uri("http://valid-issuer/"),
                    Request = new() { AccessToken = token },
                };
                context.Options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                context.Options.TokenValidationParameters.ValidateTokenReplay = false;
                context.Options.TokenValidationParameters.RequireExpirationTime = false;
                context.Options.TokenValidationParameters.RequireSignedTokens = false;

                var result = service.ExtractPrincipalFromRequestContext(context);

                result.Identity!.Name.Should().Be("test-user");
            }

            [Test]
            [Auto]
            public void ShouldThrowInvalidAuthExceptionIfAudienceIsInvalid(
                [Target] DefaultAuthService service
            )
            {
                var token = "eyJhbGciOiAibm9uZSIsInR5cCI6ICJhdCtqd3QifQ.eyJpc3MiOiJodHRwOi8vdmFsaWQtaXNzdWVyLyIsImF1ZCI6Imh0dHA6Ly9pbnZhbGlkLWlzc3Vlci8iLCJuYW1lIjoidGVzdC11c2VyIiwiZXhwIjo5OTk5OTk5OTk5fQ==.";
                var transaction = new OpenIddictServerTransaction { Options = new() };
                var context = new ValidateTokenRequestContext(transaction)
                {
                    Issuer = new Uri("http://valid-issuer/"),
                    Request = new() { AccessToken = token },
                };
                context.Options.TokenValidationParameters.ValidateAudience = true;
                context.Options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                context.Options.TokenValidationParameters.ValidateTokenReplay = false;
                context.Options.TokenValidationParameters.RequireExpirationTime = false;
                context.Options.TokenValidationParameters.RequireSignedTokens = false;

                Action func = () => service.ExtractPrincipalFromRequestContext(context);

                func.Should().Throw<InvalidAccessTokenException>();
            }

            [Test]
            [Auto]
            public void ShouldThrowInvalidAuthExceptionIfIssuerIsInvalid(
                [Target] DefaultAuthService service
            )
            {
                var token = "eyJhbGciOiAibm9uZSIsInR5cCI6ICJhdCtqd3QifQ.eyJpc3MiOiJodHRwOi8vaW52YWxpZC1pc3N1ZXIvIiwiYXVkIjoiaHR0cDovL3ZhbGlkLWlzc3Vlci8iLCJuYW1lIjoidGVzdC11c2VyIiwiZXhwIjo5OTk5OTk5OTk5fQ==.";
                var transaction = new OpenIddictServerTransaction { Options = new() };
                var context = new ValidateTokenRequestContext(transaction)
                {
                    Issuer = new Uri("http://valid-issuer/"),
                    Request = new() { AccessToken = token },
                };
                context.Options.TokenValidationParameters.ValidateIssuer = true;
                context.Options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                context.Options.TokenValidationParameters.ValidateTokenReplay = false;
                context.Options.TokenValidationParameters.RequireExpirationTime = false;
                context.Options.TokenValidationParameters.RequireSignedTokens = false;

                Action func = () => service.ExtractPrincipalFromRequestContext(context);

                func.Should().Throw<InvalidAccessTokenException>();
            }

            [Test]
            [Auto]
            public void ShouldThrowInvalidAuthExceptionIfExpired(
                [Target] DefaultAuthService service
            )
            {
                var token = "eyJhbGciOiAibm9uZSIsInR5cCI6ICJhdCtqd3QifQ.eyJpc3MiOiJodHRwOi8vdmFsaWQtaXNzdWVyLyIsImF1ZCI6Imh0dHA6Ly92YWxpZC1pc3N1ZXIvIiwibmFtZSI6InRlc3QtdXNlciIsImV4cCI6MH0=.";
                var transaction = new OpenIddictServerTransaction { Options = new() };
                var context = new ValidateTokenRequestContext(transaction)
                {
                    Issuer = new Uri("http://valid-issuer/"),
                    Request = new() { AccessToken = token },
                };
                context.Options.TokenValidationParameters.ValidateIssuer = true;
                context.Options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                context.Options.TokenValidationParameters.ValidateTokenReplay = false;
                context.Options.TokenValidationParameters.RequireExpirationTime = false;
                context.Options.TokenValidationParameters.RequireSignedTokens = false;

                Action func = () => service.ExtractPrincipalFromRequestContext(context);

                func.Should().Throw<InvalidAccessTokenException>();
            }
        }
    }
}
