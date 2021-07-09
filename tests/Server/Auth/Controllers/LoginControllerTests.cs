using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Users;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using NUnit.Framework;

using static NSubstitute.Arg;

using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;

namespace Brighid.Identity.Auth
{
    [Category("Unit")]
    public class LoginControllerTests
    {
        [Category("Unit")]
        public class Render
        {
            [Test]
            [Auto]
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

            [Test]
            [Auto]
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

            [Test]
            [Auto]
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

            [Test]
            [Auto]
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

        [Category("Unit")]
        public class Login
        {
            [Test]
            [Auto]
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

                var result = await loginController.Login(request) as ViewResult;

                result!.Should().NotBeNull();
                result!.ViewName.Should().Be("~/Auth/Views/Login.cshtml");
                result!.Model.As<LoginRequest>().RedirectUri.Should().Be(destination);
            }

            [Test]
            [Auto]
            public async Task ShouldAddModelError_IfSigninFails(
                string destination,
                LoginRequest request,
                [Frozen] HttpContext httpContext,
                [Frozen, Substitute] SignInManager<User> signInManager,
                [Frozen, Substitute] IAuthService authService,
                [Target] LoginController loginController
            )
            {
                request.RedirectUri = new Uri(destination, UriKind.Relative);
                authService.PasswordExchange(Any<string>(), Any<string>(), Any<Uri>(), Any<HttpContext>(), Any<CancellationToken>()).Throws<InvalidCredentialsException>();
                signInManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);
                var tempDataProvider = Substitute.For<ITempDataProvider>();
                var tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
                var tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

                loginController.TempData = tempData;
                loginController.ControllerContext = new ControllerContext { HttpContext = httpContext };
                var result = await loginController.Login(request) as ViewResult;

                var errors = loginController.ModelState["loginErrors"].Errors;
                result!.Should().NotBeNull();
                result!.ViewName.Should().Be("~/Auth/Views/Login.cshtml");
                errors.Should().Contain(error => error.ErrorMessage == "Username and/or password were incorrect.");
            }

            [Test]
            [Auto]
            public async Task ShouldReturnASignInResultOnSuccess(
                string destination,
                LoginRequest request,
                [Frozen, Substitute] HttpContext httpContext,
                [Target] LoginController loginController
            )
            {
                loginController.ControllerContext = new ControllerContext { HttpContext = httpContext };
                request.RedirectUri = new Uri(destination, UriKind.Relative);

                var result = await loginController.Login(request);
                result!.Should().BeOfType<SignInResult>();
            }
        }
    }
}
