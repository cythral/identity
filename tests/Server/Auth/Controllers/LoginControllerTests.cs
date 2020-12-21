using System;
using System.Security.Claims;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Users;

using FluentAssertions;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Brighid.Identity.Auth
{
    public class LoginControllerTests
    {
        public class Render
        {
            [Test, Auto]
            public void ShouldRedirect_IfAlreadySignedIn(
                [Frozen, Substitute] SignInManager<User> signinManager,
                [Target] LoginController loginController
            )
            {
                signinManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(true);

                var result = loginController.Render() as LocalRedirectResult;

                result!.Should().NotBeNull();
                result!.Url.Should().Be("/");
            }

            [Test, Auto]
            public void ShouldRedirectToGivenUrl_IfAlreadySignedIn(
                string destination,
                [Frozen, Substitute] SignInManager<User> signinManager,
                [Target] LoginController loginController
            )
            {
                signinManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(true);

                var result = loginController.Render(destination) as LocalRedirectResult;

                result!.Should().NotBeNull();
                result!.Url.Should().Be(destination);
            }

            [Test, Auto]
            public void ShouldRenderLogin_WithDefaultDestination_IfAlreadySignedIn(
                [Frozen, Substitute] SignInManager<User> signinManager,
                [Target] LoginController loginController
            )
            {
                signinManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);

                var result = loginController.Render() as ViewResult;

                result!.Should().NotBeNull();
                result!.ViewName.Should().Be("~/Auth/Views/Login.cshtml");
                result!.Model.As<LoginRequest>().RedirectUri.Should().Be("/");
            }

            [Test, Auto]
            public void ShouldRenderLogin_WithGivenRedirectUri(
                string destination,
                [Frozen, Substitute] SignInManager<User> signinManager,
                [Target] LoginController loginController
            )
            {
                signinManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);

                var result = loginController.Render(destination) as ViewResult;

                result!.Should().NotBeNull();
                result!.ViewName.Should().Be("~/Auth/Views/Login.cshtml");
                result!.Model.As<LoginRequest>().RedirectUri.Should().Be(destination);
            }
        }

        public class Login
        {
            [Test, Auto]
            public async Task ShouldRender_IfModelStateIsInvalid(
                string destination,
                LoginRequest request,
                [Frozen, Substitute] SignInManager<User> signInManager,
                [Target] LoginController loginController
            )
            {
                request.RedirectUri = new Uri(destination, UriKind.Relative);
                loginController.ModelState.AddModelError("loginError", "Invalid Model State");
                signInManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);
                signInManager.PasswordSignInAsync(Any<string>(), Any<string>(), Any<bool>(), Any<bool>()).Returns(SignInResult.Success);

                var result = await loginController.Login(request) as ViewResult;

                result!.Should().NotBeNull();
                result!.ViewName.Should().Be("~/Auth/Views/Login.cshtml");
                result!.Model.As<LoginRequest>().RedirectUri.Should().Be(destination);
            }

            [Test, Auto]
            public async Task ShouldAddModelError_IfSigninFails(
                string destination,
                LoginRequest request,
                [Frozen, Substitute] SignInManager<User> signInManager,
                [Target] LoginController loginController
            )
            {
                request.RedirectUri = new Uri(destination, UriKind.Relative);
                signInManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);
                signInManager.PasswordSignInAsync(Any<string>(), Any<string>(), Any<bool>(), Any<bool>()).Returns(SignInResult.Failed);

                var result = await loginController.Login(request) as ViewResult;

                await signInManager.Received().PasswordSignInAsync(Is(request.Username), Is(request.Password), Is(false), Is(false));
                var errors = loginController.ModelState["loginErrors"].Errors;

                result!.Should().NotBeNull();
                result!.ViewName.Should().Be("~/Auth/Views/Login.cshtml");
                errors.Should().Contain(error => error.ErrorMessage == "Username and/or password were incorrect.");
            }

            [Test, Auto]
            public async Task ShouldRedirectToDestination_IfLoginSucceeds(
                string destination,
                LoginRequest request,
                [Frozen, Substitute] SignInManager<User> signInManager,
                [Target] LoginController loginController
            )
            {
                request.RedirectUri = new Uri(destination, UriKind.Relative);

                signInManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);
                signInManager.PasswordSignInAsync(Any<string>(), Any<string>(), Any<bool>(), Any<bool>()).Returns(SignInResult.Success);

                var result = await loginController.Login(request) as LocalRedirectResult;
                result!.Should().NotBeNull();
                result!.Url.Should().Be(destination);
            }
        }
    }
}
