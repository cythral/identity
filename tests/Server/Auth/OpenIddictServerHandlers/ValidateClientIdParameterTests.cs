using System;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using OpenIddict.Abstractions;

using static OpenIddict.Server.OpenIddictServerEvents;

namespace Brighid.Identity.Auth
{
    public class ValidateClientIdParameterTests
    {
        [TestFixture]
        [Category("Unit")]
        public class HandleAsyncTests
        {
            [Test]
            [Auto]
            public async Task ShouldThrowIfContextIsNull(
                [Target] ValidateClientIdParameter validator
            )
            {
                Func<Task> func = async () => await validator.HandleAsync(null!);

                (await func.Should().ThrowAsync<ArgumentNullException>()).And.ParamName.Should().Be("context");
            }

            [Test]
            [Auto]
            public async Task ShouldNotRejectWhenClientIdIsNotNull(
                string clientId,
                ValidateTokenRequestContext context,
                [Target] ValidateClientIdParameter validator
            )
            {
                context.Request.ClientId = clientId;
                context.Options.AcceptAnonymousClients = false;

                await validator.HandleAsync(context);

                context.IsRejected.Should().BeFalse();
            }

            [Test]
            [Auto]
            public async Task ShouldNotRejectWhenGrantTypeIsImpersonate(
                ValidateTokenRequestContext context,
                [Target] ValidateClientIdParameter validator
            )
            {
                context.Request.ClientId = null;
                context.Request.GrantType = Constants.GrantTypes.Impersonate;
                context.Options.AcceptAnonymousClients = false;

                await validator.HandleAsync(context);

                context.IsRejected.Should().BeFalse();
            }

            [Test]
            [Auto]
            public async Task ShouldNotRejectWhenAcceptingAnonymousClients(
                ValidateTokenRequestContext context,
                [Target] ValidateClientIdParameter validator
            )
            {
                context.Request.ClientId = null;
                context.Request.GrantType = "token";
                context.Options.AcceptAnonymousClients = true;

                await validator.HandleAsync(context);

                context.IsRejected.Should().BeFalse();
            }

            [Test]
            [Auto]
            public async Task ShouldRejectWhenGrantTypeIsCode(
                ValidateTokenRequestContext context,
                [Target] ValidateClientIdParameter validator
            )
            {
                context.Request.ClientId = null;
                context.Request.GrantType = OpenIddictConstants.GrantTypes.AuthorizationCode;
                context.Options.AcceptAnonymousClients = true;

                await validator.HandleAsync(context);

                context.IsRejected.Should().BeTrue();
            }
        }
    }
}
