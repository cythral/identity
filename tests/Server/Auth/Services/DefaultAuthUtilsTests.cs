using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Applications;
using Brighid.Identity.Roles;
using Brighid.Identity.Users;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

using static NSubstitute.Arg;
using static OpenIddict.Abstractions.OpenIddictConstants;

#pragma warning disable IDE0120
namespace Brighid.Identity.Auth
{
    [Category("Unit")]
    public class DefaultAuthUtilsTests
    {
        [Category("Unit")]
        public class CreateClaimsIdentityForApplication
        {
            [Test, Auto]
            public async Task ShouldSetNameClaim(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForApplication(id);

                var nameClaim = result.GetClaim(Claims.Name);
                nameClaim.Should().Be(id.ToString());
            }

            [Test, Auto]
            public async Task Name_ShouldHaveAccessTokenDestination(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForApplication(id);
                var nameClaim = result.Claims.Where(claim => claim.Type == Claims.Name).First();
                var destinations = nameClaim.GetDestinations();

                destinations.Should().Contain(Destinations.AccessToken);
            }

            [Test, Auto]
            public async Task Name_ShouldHaveIdentityTokenDestination(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForApplication(id);
                var nameClaim = result.Claims.Where(claim => claim.Type == Claims.Name).First();
                var destinations = nameClaim.GetDestinations();

                destinations.Should().Contain(Destinations.IdentityToken);
            }

            [Test, Auto]
            public async Task ShouldSetSubjectClaim(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForApplication(id);

                var subjectClaim = result.GetClaim(Claims.Subject);
                subjectClaim.Should().Be(id.ToString());
            }

            [Test, Auto]
            public async Task Subject_ShouldHaveAccessTokenDestination(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForApplication(id);
                var subjectClaim = result.Claims.Where(claim => claim.Type == Claims.Subject).First();
                var destinations = subjectClaim.GetDestinations();

                destinations.Should().Contain(Destinations.AccessToken);
            }

            [Test, Auto]
            public async Task Subject_ShouldHaveIdentityTokenDestination(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForApplication(id);
                var subjectClaim = result.Claims.Where(claim => claim.Type == Claims.Subject).First();
                var destinations = subjectClaim.GetDestinations();

                destinations.Should().Contain(Destinations.IdentityToken);
            }

            [Test, Auto]
            public async Task Role_ShouldBeJsonArray(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForApplication(id);
                var roleClaim = result.Claims.Where(claim => claim.Type == Claims.Role).First();

                roleClaim.ValueType.Should().Be(JsonClaimValueTypes.JsonArray);
            }

            [Test, Auto]
            public async Task Role_ShouldHaveAccessTokenDestination(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForApplication(id);
                var roleClaim = result.Claims.Where(claim => claim.Type == Claims.Role).First();
                var destinations = roleClaim.GetDestinations();

                destinations.Should().Contain(Destinations.AccessToken);
            }

            [Test, Auto]
            public async Task Role_ShouldHaveIdentityTokenDestination(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForApplication(id);
                var roleClaim = result.Claims.Where(claim => claim.Type == Claims.Role).First();
                var destinations = roleClaim.GetDestinations();

                destinations.Should().Contain(Destinations.IdentityToken);
            }

            [Test, Auto]
            public async Task AuthenticationScheme_ShouldBeSetToDefault(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForApplication(id);
                var scheme = result.AuthenticationType;

                scheme.Should().Be(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            [Test, Auto]
            public async Task NameClaimType_ShouldBeSet(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForApplication(id);
                var nameClaimType = result.NameClaimType;

                nameClaimType.Should().Be(Claims.Name);
            }

            [Test, Auto]
            public async Task RoleType_ShouldBeSet(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForApplication(id);
                var roleClaimType = result.RoleClaimType;

                roleClaimType.Should().Be(Claims.Role);
            }
        }

        [Category("Unit")]
        public class CreateClaimsIdentityForUser
        {
            [Test, Auto]
            public async Task ShouldSetNameClaim(
                User user,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForUser(user);

                var nameClaim = result.GetClaim(Claims.Name);
                nameClaim.Should().Be(user.Email.ToString());
            }

            [Test, Auto]
            public async Task Name_ShouldHaveAccessTokenDestination(
                User user,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForUser(user);
                var nameClaim = result.Claims.Where(claim => claim.Type == Claims.Name).First();
                var destinations = nameClaim.GetDestinations();

                destinations.Should().Contain(Destinations.AccessToken);
            }

            [Test, Auto]
            public async Task Name_ShouldHaveIdentityTokenDestination(
                User user,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForUser(user);
                var nameClaim = result.Claims.Where(claim => claim.Type == Claims.Name).First();
                var destinations = nameClaim.GetDestinations();

                destinations.Should().Contain(Destinations.IdentityToken);
            }

            [Test, Auto]
            public async Task ShouldSetSubjectClaim(
                User user,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForUser(user);

                var nameClaim = result.GetClaim(Claims.Subject);
                nameClaim.Should().Be(user.Id.ToString());
            }

            [Test, Auto]
            public async Task Subject_ShouldHaveAccessTokenDestination(
                User user,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForUser(user);
                var nameClaim = result.Claims.Where(claim => claim.Type == Claims.Subject).First();
                var destinations = nameClaim.GetDestinations();

                destinations.Should().Contain(Destinations.AccessToken);
            }

            [Test, Auto]
            public async Task Subject_ShouldHaveIdentityTokenDestination(
                User user,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForUser(user);
                var nameClaim = result.Claims.Where(claim => claim.Type == Claims.Subject).First();
                var destinations = nameClaim.GetDestinations();

                destinations.Should().Contain(Destinations.IdentityToken);
            }

            [Test, Auto]
            public async Task Role_ShouldBeJsonArray(
                User user,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForUser(user);
                var roleClaim = result.Claims.Where(claim => claim.Type == Claims.Role).First();

                roleClaim.ValueType.Should().Be(JsonClaimValueTypes.JsonArray);
            }

            [Test, Auto]
            public async Task Role_ShouldHaveAccessTokenDestination(
                User user,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForUser(user);
                var roleClaim = result.Claims.Where(claim => claim.Type == Claims.Role).First();
                var destinations = roleClaim.GetDestinations();

                destinations.Should().Contain(Destinations.AccessToken);
            }

            [Test, Auto]
            public async Task Role_ShouldHaveIdentityTokenDestination(
                User user,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentityForUser(user);
                var roleClaim = result.Claims.Where(claim => claim.Type == Claims.Role).First();
                var destinations = roleClaim.GetDestinations();

                destinations.Should().Contain(Destinations.IdentityToken);
            }

            [Test, Auto]
            public async Task Role_ShouldContainAllTheUsersRoles(
                User user,
                Role role1,
                Role role2,
                [Frozen, Substitute] IUserRepository repository,
                [Target] DefaultAuthUtils authUtils,
                CancellationToken cancellationToken
            )
            {
                repository.FindRolesById(Any<Guid>(), Any<CancellationToken>()).Returns(new[] { role1, role2 });
                var result = await authUtils.CreateClaimsIdentityForUser(user, cancellationToken);
                var roleClaim = result.Claims.Where(claim => claim.Type == Claims.Role).First();
                var roleNames = JsonSerializer.Deserialize<string[]>(roleClaim.Value);

                roleNames.Should().Contain(role1.Name);
                roleNames.Should().Contain(role2.Name);

                await repository.Received().FindRolesById(Is(user.Id), Is(cancellationToken));
            }
        }

        [Category("Unit")]
        public class CreateAuthTicket
        {
            [Test, Auto]
            public void Principal_ShouldContainClaimsIdentity(
                ClaimsIdentity claimsIdentity,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = authUtils.CreateAuthTicket(claimsIdentity);
                var identities = result.Principal.Identities;

                identities.Should().Contain(claimsIdentity);
            }

            [Test, Auto]
            public void Properties_ShouldHaveRedirectUri(
                ClaimsIdentity claimsIdentity,
                Uri redirectUri,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = authUtils.CreateAuthTicket(claimsIdentity, redirectUri: redirectUri);

                result.Properties.RedirectUri.Should().Be(redirectUri.ToString());
            }

            [Test, Auto]
            public void AuthenticationScheme_ShouldBeSet(
                ClaimsIdentity claimsIdentity,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = authUtils.CreateAuthTicket(claimsIdentity);
                var scheme = result.AuthenticationScheme;

                scheme.Should().Be(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            [Test, Auto]
            public void AuthenticationScheme_ShouldBeSetToGivenScheme(
                string givenScheme,
                ClaimsIdentity claimsIdentity,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = authUtils.CreateAuthTicket(claimsIdentity, authenticationScheme: givenScheme);
                var scheme = result.AuthenticationScheme;

                scheme.Should().Be(givenScheme);
            }

            [Test, Auto]
            public void Resources_ShouldContainDomain(
                ClaimsIdentity claimsIdentity,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = authUtils.CreateAuthTicket(claimsIdentity);
                var resources = result.Principal.GetResources();

                resources.Should().Contain("identity.brigh.id");
            }

            [Test, Auto]
            public void Scopes_ShouldBeSet(
                string[] scopes,
                ClaimsIdentity claimsIdentity,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = authUtils.CreateAuthTicket(claimsIdentity, scopes);
                var actualScopes = result.Principal.GetScopes();

                actualScopes.Should().BeEquivalentTo(scopes);
            }
        }
    }
}
