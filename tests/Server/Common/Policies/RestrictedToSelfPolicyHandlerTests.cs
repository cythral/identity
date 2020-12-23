using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

using AspNet.Security.OpenIdConnect.Primitives;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Roles;


using FluentAssertions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity
{
    public static class RestrictedToSelfPolicyHandlerExtensions
    {
        private static readonly MethodInfo MethodInfo = typeof(RestrictedToSelfPolicyHandler).GetMethod("HandleRequirementAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
        private static readonly Func<RestrictedToSelfPolicyHandler, AuthorizationHandlerContext, RestrictedToSelfPolicyRequirement, Task> Delegate = (handler, context, requirement) => (Task)MethodInfo!.Invoke(handler, new object[] { context, requirement })!;

        public static Task CallHandleRequirementAsync(this RestrictedToSelfPolicyHandler handler, AuthorizationHandlerContext context, RestrictedToSelfPolicyRequirement requirement)
        {
            return Delegate(handler, context, requirement);
        }
    }

    public class RestrictedToSelfPolicyHandlerTests
    {

        [Test, Auto]
        public async Task ShouldSucceed_IfResourceIsNotHttpContext(
            object resource,
            RestrictedToSelfPolicyRequirement requirement,
            [Substitute] AuthorizationHandlerContext context,
            [Target] RestrictedToSelfPolicyHandler handler
        )
        {
            context.Resource.Returns(resource);

            await handler.CallHandleRequirementAsync(context, requirement);

            context.Received().Succeed(Is(requirement));
            context.DidNotReceive().Fail();
        }

        [Test, Auto]
        public async Task ShouldFail_IfResourceIsEndpoint_AndClaimDoesntMatchValueFromRoute(
            string routeUserId,
            string loggedInUserId,
            [Substitute, Frozen] Func<string?, object?> parse,
            RestrictedToSelfPolicyRequirement requirement,
            [Substitute] HttpContext httpContext,
            [Substitute] AuthorizationHandlerContext context,
            [Target] RestrictedToSelfPolicyHandler handler
        )
        {
            parse(Any<string?>()).Returns(routeUserId);
            context.Resource.Returns(httpContext);

            var idClaim = new Claim(requirement.ClaimType, loggedInUserId);
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);
            identity.AddClaim(idClaim);
            httpContext.User = principal;
            httpContext.Request.RouteValues.Returns(new RouteValueDictionary { [requirement.RouteParameter] = routeUserId });

            await handler.CallHandleRequirementAsync(context, requirement);

            context.Received().Fail();
            context.DidNotReceive().Succeed(Any<IAuthorizationRequirement>());
        }

        [Test, Auto]
        public async Task ShouldFail_IfResourceIsEndpoint_AndRouteValueIsNull(
            [Substitute, Frozen] Func<string?, object?> parse,
            RestrictedToSelfPolicyRequirement requirement,
            [Substitute] HttpContext httpContext,
            [Substitute] AuthorizationHandlerContext context,
            [Target] RestrictedToSelfPolicyHandler handler
        )
        {
            parse(Any<string?>()).Returns(null);
            context.Resource.Returns(httpContext);
            httpContext.Request.RouteValues.Returns(new RouteValueDictionary { [requirement.RouteParameter] = null });

            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);
            context.User.Returns(principal);

            await handler.CallHandleRequirementAsync(context, requirement);

            context.Received().Fail();
            context.DidNotReceive().Succeed(Any<IAuthorizationRequirement>());
        }

        [Test, Auto]
        public async Task ShouldSucceed_IfResourceIsEndpoint_AndClaimMatchesValueFromRoute(
            string userId,
            string parsedUserId,
            [Substitute, Frozen] Func<string?, object?> parse,
            RestrictedToSelfPolicyRequirement requirement,
            [Substitute] HttpContext httpContext,
            [Substitute] AuthorizationHandlerContext context,
            [Target] RestrictedToSelfPolicyHandler handler
        )
        {
            parse(Any<string?>()).Returns(parsedUserId);
            context.Resource.Returns(httpContext);
            httpContext.Request.RouteValues.Returns(new RouteValueDictionary { [requirement.RouteParameter] = userId });

            var idClaim = new Claim(requirement.ClaimType, parsedUserId);
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);
            identity.AddClaim(idClaim);
            context.User.Returns(principal);

            await handler.CallHandleRequirementAsync(context, requirement);

            parse.Received()(Is(userId));
            context.Received().Succeed(Is(requirement));
            context.DidNotReceive().Fail();
        }

        [Test, Auto]
        public async Task ShouldSucceed_IfResourceIsEndpoint_IfIdsAreMismatched_ButUserIsAdministrator(
            string userId,
            string mismatchedUserId,
            [Substitute, Frozen] Func<string?, object?> parse,
            RestrictedToSelfPolicyRequirement requirement,
            [Substitute] HttpContext httpContext,
            [Substitute] AuthorizationHandlerContext context,
            [Target] RestrictedToSelfPolicyHandler handler
        )
        {
            parse(Any<string?>()).Returns(mismatchedUserId);
            context.Resource.Returns(httpContext);
            httpContext.Request.RouteValues.Returns(new RouteValueDictionary { [requirement.RouteParameter] = userId });

            var idClaim = new Claim(requirement.ClaimType, userId);
            var roleClaim = new Claim(Constants.ClaimTypes.Role, nameof(BuiltInRole.Administrator));
            var identity = new ClaimsIdentity(null, null, Constants.ClaimTypes.Role);
            var principal = new ClaimsPrincipal(identity);
            identity.AddClaim(idClaim);
            identity.AddClaim(roleClaim);
            context.User.Returns(principal);

            await handler.CallHandleRequirementAsync(context, requirement);

            context.Received().Succeed(Is(requirement));
            context.DidNotReceive().Fail();
        }
    }
}
