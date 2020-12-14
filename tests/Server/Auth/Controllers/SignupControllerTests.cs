using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AspNet.Security.OpenIdConnect.Primitives;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Users;

using FluentAssertions;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.Auth
{
    public class SignupControllerTests
    {
        public class Render
        {
            [Test, Auto]
            public void ShouldRedirect_IfAlreadySignedIn(
                [Frozen, Substitute] SignInManager<User> signinManager,
                [Target] SignupController signupController
            )
            {
                signinManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(true);

                var result = signupController.Render() as LocalRedirectResult;

                result!.Should().NotBeNull();
                result!.Url.Should().Be("/");
            }

            [Test, Auto]
            public void ShouldRedirectToGivenUrl_IfAlreadySignedIn(
                string destination,
                [Frozen, Substitute] SignInManager<User> signinManager,
                [Target] SignupController signupController
            )
            {
                signinManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(true);

                var result = signupController.Render(destination) as LocalRedirectResult;

                result!.Should().NotBeNull();
                result!.Url.Should().Be(destination);
            }

            [Test, Auto]
            public void ShouldRenderSignup_WithDefaultDestination_IfAlreadySignedIn(
                [Frozen, Substitute] SignInManager<User> signinManager,
                [Target] SignupController signupController
            )
            {
                signinManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);

                var result = signupController.Render() as ViewResult;

                result!.Should().NotBeNull();
                result!.ViewName.Should().Be("~/Auth/Views/Signup.cshtml");
                result!.Model.As<SignupRequest>().RedirectUri.Should().Be("/");
            }

            [Test, Auto]
            public void ShouldRenderSignup_WithGivenRedirectUri(
                string destination,
                [Frozen, Substitute] SignInManager<User> signinManager,
                [Target] SignupController signupController
            )
            {
                signinManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);

                var result = signupController.Render(destination) as ViewResult;

                result!.Should().NotBeNull();
                result!.ViewName.Should().Be("~/Auth/Views/Signup.cshtml");
                result!.Model.As<SignupRequest>().RedirectUri.Should().Be(destination);
            }
        }
    }
}
