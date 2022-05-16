using System;
using System.Security.Claims;

using FluentAssertions;

using NUnit.Framework;

using OpenIddict.Abstractions;

namespace Brighid.Identity.Users
{
    public class DefaultPrincipalServiceTests
    {
        [TestFixture]
        [Category("Unit")]
        public class GetIdTests
        {
            [Test]
            [Auto]
            public void ShouldReturnTheParsedSubClaim(
                Guid userId,
                [Target] DefaultPrincipalService service
            )
            {
                var principal = new ClaimsPrincipal();
                principal.AddIdentity(new ClaimsIdentity());
                principal.SetClaim("sub", userId.ToString());

                var result = service.GetId(principal);

                result.Should().Be(userId);
            }

            [Test]
            [Auto]
            public void ShouldThrowInvalidPrincipalExceptionIfTheSubClaimIsntPresent(
                [Target] DefaultPrincipalService service
            )
            {
                var principal = new ClaimsPrincipal();
                principal.AddIdentity(new ClaimsIdentity());

                Action func = () => service.GetId(principal);

                func.Should().Throw<InvalidPrincipalException>();
            }

            [Test]
            [Auto]
            public void ShouldThrowInvalidPrincipalExceptionIfTheSubClaimIsNotAGuid(
                [Target] DefaultPrincipalService service
            )
            {
                var principal = new ClaimsPrincipal();
                principal.AddIdentity(new ClaimsIdentity());
                principal.SetClaim("sub", "notaguid");

                Action func = () => service.GetId(principal);

                func.Should().Throw<InvalidPrincipalException>();
            }
        }
    }
}
