using System;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using OpenIddict.Abstractions;

using static NSubstitute.Arg;
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
                [Substitute] ValidateTokenRequestContext context,
                [Target] ValidateClientIdParameter validator
            )
            {
                context.Request.ClientId = clientId;
                context.Options.AcceptAnonymousClients = false;

                await validator.HandleAsync(context);

                context.DidNotReceive().Reject(Any<string>(), Any<string>(), Any<string>());
            }

            [Test]
            [Auto]
            public async Task ShouldNotRejectWhenGrantTypeIsImpersonate(
                [Substitute] ValidateTokenRequestContext context,
                [Target] ValidateClientIdParameter validator
            )
            {
                context.Request.ClientId = null;
                context.Request.GrantType = Constants.GrantTypes.Impersonate;
                context.Options.AcceptAnonymousClients = false;

                await validator.HandleAsync(context);

                context.DidNotReceive().Reject(Any<string>(), Any<string>(), Any<string>());
            }

            [Test]
            [Auto]
            public async Task ShouldNotRejectWhenAcceptingAnonymousClients(
                [Substitute] ValidateTokenRequestContext context,
                [Target] ValidateClientIdParameter validator
            )
            {
                context.Request.ClientId = null;
                context.Request.GrantType = "token";
                context.Options.AcceptAnonymousClients = true;

                await validator.HandleAsync(context);

                context.DidNotReceive().Reject(Any<string>(), Any<string>(), Any<string>());
            }

            [Test]
            [Auto]
            public async Task ShouldRejectWhenGrantTypeIsCode(
                [Substitute] ValidateTokenRequestContext context,
                [Target] ValidateClientIdParameter validator
            )
            {
                context.Request.ClientId = null;
                context.Request.GrantType = OpenIddictConstants.GrantTypes.AuthorizationCode;
                context.Options.AcceptAnonymousClients = true;

                await validator.HandleAsync(context);

                context.Received().Reject(Any<string>(), Any<string>(), Any<string>());
            }
        }
    }
}
