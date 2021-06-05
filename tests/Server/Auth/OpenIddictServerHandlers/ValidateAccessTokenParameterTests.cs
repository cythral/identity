using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Roles;

using FluentAssertions;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using NUnit.Framework;

using static NSubstitute.Arg;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Brighid.Identity.Auth
{
    public class ValidateAccessTokenParameterTests
    {
        [Category("Unit")]
        public class HandleAsyncTests
        {
            [Test]
            [Auto]
            public async Task ShouldThrowIfContextIsNull(
                [Target] ValidateAccessTokenParameter validator
            )
            {
                Func<Task> func = async () => await validator.HandleAsync(null!);

                (await func.Should().ThrowAsync<ArgumentNullException>()).And.ParamName.Should().Be("context");
            }

            [Test]
            [Auto]
            public async Task ShouldRejectIfTheAccessTokenIsInvalid(
                [Substitute] ValidateTokenRequestContext context,
                [Frozen, Substitute] IAuthService authService,
                [Target] ValidateAccessTokenParameter validator
            )
            {
                authService.ExtractPrincipalFromRequestContext(Any<ValidateTokenRequestContext>()).Throws<InvalidAccessTokenException>();
                context.Request.GrantType = Constants.GrantTypes.Impersonate;

                await validator.HandleAsync(context);

                context.Received().Reject(Any<string>(), Any<string>(), Any<string>());
                authService.Received().ExtractPrincipalFromRequestContext(Is(context));
            }

            [Test]
            [Auto]
            public async Task ShouldSetTheContextPrincipal(
                ClaimsPrincipal principal,
                [Substitute] ValidateTokenRequestContext context,
                [Frozen, Substitute] IAuthService authService,
                [Target] ValidateAccessTokenParameter validator
            )
            {
                authService.ExtractPrincipalFromRequestContext(Any<ValidateTokenRequestContext>()).Returns(principal);
                context.Request.GrantType = Constants.GrantTypes.Impersonate;

                await validator.HandleAsync(context);

                context.Principal.Should().Be(principal);
            }

            [Test]
            [Auto]
            public async Task ShouldRejectIfUserDoesntHaveImpersonatorRole(
                [Frozen] ClaimsPrincipal principal,
                [Substitute] ValidateTokenRequestContext context,
                [Frozen, Substitute] IRoleService roleService,
                [Target] ValidateAccessTokenParameter validator
            )
            {
                roleService.When(svc => svc.ValidateUserHasRoles(Any<IEnumerable<string>>(), Any<ClaimsPrincipal>())).Throw(new RoleRequiredException(nameof(BuiltInRole.Impersonator)));
                context.Request.GrantType = Constants.GrantTypes.Impersonate;

                await validator.HandleAsync(context);

                context.Received().Reject(Any<string>(), Any<string>(), Any<string>());
                roleService.Received().ValidateUserHasRoles(Is<IEnumerable<string>>(roles => roles.Contains(nameof(BuiltInRole.Impersonator))), Is(principal));
            }

            [Test]
            [Auto]
            public async Task ShouldNotExtractPrincipalIfGrantTypeIsNotImpersonate(
                [Substitute] ValidateTokenRequestContext context,
                [Frozen, Substitute] IAuthService authService,
                [Target] ValidateAccessTokenParameter validator
            )
            {
                context.Request.GrantType = "not impersonate";

                await validator.HandleAsync(context);

                authService.DidNotReceive().ExtractPrincipalFromRequestContext(Any<ValidateTokenRequestContext>());
            }
        }
    }
}
