using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;

using AutoFixture.NUnit3;

using Brighid.Identity.Applications;
using Brighid.Identity.Roles;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using OpenIddict.Server;

using static NSubstitute.Arg;

namespace Brighid.Identity.Auth
{
    [Category("Unit")]
    public class DefaultAuthUtilsTests
    {
        [Category("Unit")]
        public class CreateClaimsIdentity
        {
            [Test, Auto]
            public async Task ShouldSetNameClaim(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(id);

                var nameClaim = result.GetClaim(OpenIdConnectConstants.Claims.Name);
                nameClaim.Should().Be(id.ToString());
            }

            [Test, Auto]
            public async Task Name_ShouldHaveAccessTokenDestination(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(id);
                var nameClaim = result.Claims.Where(claim => claim.Type == OpenIdConnectConstants.Claims.Name).First();
                var destinations = nameClaim.GetDestinations();

                destinations.Should().Contain(OpenIdConnectConstants.Destinations.AccessToken);
            }

            [Test, Auto]
            public async Task Name_ShouldHaveIdentityTokenDestination(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(id);
                var nameClaim = result.Claims.Where(claim => claim.Type == OpenIdConnectConstants.Claims.Name).First();
                var destinations = nameClaim.GetDestinations();

                destinations.Should().Contain(OpenIdConnectConstants.Destinations.IdentityToken);
            }

            [Test, Auto]
            public async Task ShouldSetSubjectClaim(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(id);

                var subjectClaim = result.GetClaim(OpenIdConnectConstants.Claims.Subject);
                subjectClaim.Should().Be(id.ToString());
            }

            [Test, Auto]
            public async Task Subject_ShouldHaveAccessTokenDestination(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(id);
                var subjectClaim = result.Claims.Where(claim => claim.Type == OpenIdConnectConstants.Claims.Subject).First();
                var destinations = subjectClaim.GetDestinations();

                destinations.Should().Contain(OpenIdConnectConstants.Destinations.AccessToken);
            }

            [Test, Auto]
            public async Task Subject_ShouldHaveIdentityTokenDestination(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(id);
                var subjectClaim = result.Claims.Where(claim => claim.Type == OpenIdConnectConstants.Claims.Subject).First();
                var destinations = subjectClaim.GetDestinations();

                destinations.Should().Contain(OpenIdConnectConstants.Destinations.IdentityToken);
            }

            [Test, Auto]
            public async Task Should_SetRoleClaim(
                Guid id,
                Role role1,
                Role role2,
                [Frozen] IApplicationRoleRepository roleRepository,
                [Target] DefaultAuthUtils authUtils
            )
            {
                roleRepository.FindRolesForApplication(Any<Guid>()).Returns(new Role[] { role1, role2 });

                var result = await authUtils.CreateClaimsIdentity(id);
                var roleClaim = result.GetClaim(OpenIdConnectConstants.Claims.Role);

                roleClaim.Should().Be($"[\"{role1.Name}\",\"{role2.Name}\"]");
                await roleRepository.Received().FindRolesForApplication(Is(id));
            }

            [Test, Auto]
            public async Task Role_ShouldBeJsonArray(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(id);
                var roleClaim = result.Claims.Where(claim => claim.Type == OpenIdConnectConstants.Claims.Role).First();

                roleClaim.ValueType.Should().Be(JsonClaimValueTypes.JsonArray);
            }

            [Test, Auto]
            public async Task Role_ShouldHaveAccessTokenDestination(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(id);
                var roleClaim = result.Claims.Where(claim => claim.Type == OpenIdConnectConstants.Claims.Role).First();
                var destinations = roleClaim.GetDestinations();

                destinations.Should().Contain(OpenIdConnectConstants.Destinations.AccessToken);
            }

            [Test, Auto]
            public async Task Role_ShouldHaveIdentityTokenDestination(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(id);
                var roleClaim = result.Claims.Where(claim => claim.Type == OpenIdConnectConstants.Claims.Role).First();
                var destinations = roleClaim.GetDestinations();

                destinations.Should().Contain(OpenIdConnectConstants.Destinations.IdentityToken);
            }

            [Test, Auto]
            public async Task AuthenticationScheme_ShouldBeSetToDefault(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(id);
                var scheme = result.AuthenticationType;

                scheme.Should().Be(OpenIddictServerDefaults.AuthenticationScheme);
            }

            [Test, Auto]
            public async Task NameClaimType_ShouldBeSet(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(id);
                var nameClaimType = result.NameClaimType;

                nameClaimType.Should().Be(OpenIdConnectConstants.Claims.Name);
            }

            [Test, Auto]
            public async Task RoleType_ShouldBeSet(
                Guid id,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(id);
                var roleClaimType = result.RoleClaimType;

                roleClaimType.Should().Be(OpenIdConnectConstants.Claims.Role);
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
            public void AuthenticationScheme_ShouldBeSet(
                ClaimsIdentity claimsIdentity,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = authUtils.CreateAuthTicket(claimsIdentity);
                var scheme = result.AuthenticationScheme;

                scheme.Should().Be(OpenIddictServerDefaults.AuthenticationScheme);
            }

            [Test, Auto]
            public void Resources_ShouldContainDomain(
                ClaimsIdentity claimsIdentity,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = authUtils.CreateAuthTicket(claimsIdentity);
                var resources = result.GetResources();

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
                var actualScopes = result.GetScopes();

                actualScopes.Should().BeEquivalentTo(scopes);
            }
        }
    }
}
