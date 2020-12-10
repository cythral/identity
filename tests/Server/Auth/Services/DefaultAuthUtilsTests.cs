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
    public class DefaultAuthUtilsTests
    {
        public class CreateClaimsIdentity
        {
            [Test, Auto]
            public async Task ShouldSetNameClaim(
                string name,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(name);

                var nameClaim = result.GetClaim(OpenIdConnectConstants.Claims.Name);
                nameClaim.Should().Be(name);
            }

            [Test, Auto]
            public async Task Name_ShouldHaveAccessTokenDestination(
                string name,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(name);
                var nameClaim = result.Claims.Where(claim => claim.Type == OpenIdConnectConstants.Claims.Name).First();
                var destinations = nameClaim.GetDestinations();

                destinations.Should().Contain(OpenIdConnectConstants.Destinations.AccessToken);
            }

            [Test, Auto]
            public async Task Name_ShouldHaveIdentityTokenDestination(
                string name,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(name);
                var nameClaim = result.Claims.Where(claim => claim.Type == OpenIdConnectConstants.Claims.Name).First();
                var destinations = nameClaim.GetDestinations();

                destinations.Should().Contain(OpenIdConnectConstants.Destinations.IdentityToken);
            }

            [Test, Auto]
            public async Task ShouldSetSubjectClaim(
                string name,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(name);

                var subjectClaim = result.GetClaim(OpenIdConnectConstants.Claims.Subject);
                subjectClaim.Should().Be($"{name}@identity.brigh.id");
            }

            [Test, Auto]
            public async Task Subject_ShouldHaveAccessTokenDestination(
                string name,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(name);
                var subjectClaim = result.Claims.Where(claim => claim.Type == OpenIdConnectConstants.Claims.Subject).First();
                var destinations = subjectClaim.GetDestinations();

                destinations.Should().Contain(OpenIdConnectConstants.Destinations.AccessToken);
            }

            [Test, Auto]
            public async Task Subject_ShouldHaveIdentityTokenDestination(
                string name,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(name);
                var subjectClaim = result.Claims.Where(claim => claim.Type == OpenIdConnectConstants.Claims.Subject).First();
                var destinations = subjectClaim.GetDestinations();

                destinations.Should().Contain(OpenIdConnectConstants.Destinations.IdentityToken);
            }

            [Test, Auto]
            public async Task Should_SetRoleClaim(
                string name,
                Role role1,
                Role role2,
                [Frozen] IApplicationRoleRepository roleRepository,
                [Target] DefaultAuthUtils authUtils
            )
            {
                roleRepository.FindRolesForApplication(Any<string>()).Returns(new Role[] { role1, role2 });

                var result = await authUtils.CreateClaimsIdentity(name);
                var roleClaim = result.GetClaim(OpenIdConnectConstants.Claims.Role);

                roleClaim.Should().Be($"[\"{role1.Name}\",\"{role2.Name}\"]");
                await roleRepository.Received().FindRolesForApplication(Is(name));
            }

            [Test, Auto]
            public async Task Role_ShouldBeJsonArray(
                string name,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(name);
                var roleClaim = result.Claims.Where(claim => claim.Type == OpenIdConnectConstants.Claims.Role).First();

                roleClaim.ValueType.Should().Be(JsonClaimValueTypes.JsonArray);
            }

            [Test, Auto]
            public async Task Role_ShouldHaveAccessTokenDestination(
                string name,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(name);
                var roleClaim = result.Claims.Where(claim => claim.Type == OpenIdConnectConstants.Claims.Role).First();
                var destinations = roleClaim.GetDestinations();

                destinations.Should().Contain(OpenIdConnectConstants.Destinations.AccessToken);
            }

            [Test, Auto]
            public async Task Role_ShouldHaveIdentityTokenDestination(
                string name,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(name);
                var roleClaim = result.Claims.Where(claim => claim.Type == OpenIdConnectConstants.Claims.Role).First();
                var destinations = roleClaim.GetDestinations();

                destinations.Should().Contain(OpenIdConnectConstants.Destinations.IdentityToken);
            }

            [Test, Auto]
            public async Task AuthenticationScheme_ShouldBeSetToDefault(
                string name,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(name);
                var scheme = result.AuthenticationType;

                scheme.Should().Be(OpenIddictServerDefaults.AuthenticationScheme);
            }

            [Test, Auto]
            public async Task NameClaimType_ShouldBeSet(
                string name,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(name);
                var nameClaimType = result.NameClaimType;

                nameClaimType.Should().Be(OpenIdConnectConstants.Claims.Name);
            }

            [Test, Auto]
            public async Task RoleType_ShouldBeSet(
                string name,
                [Target] DefaultAuthUtils authUtils
            )
            {
                var result = await authUtils.CreateClaimsIdentity(name);
                var roleClaimType = result.RoleClaimType;

                roleClaimType.Should().Be(OpenIdConnectConstants.Claims.Role);
            }
        }

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
