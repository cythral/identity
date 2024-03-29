using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Users;

using FluentAssertions;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using NUnit.Framework;

using static NSubstitute.Arg;

using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;

namespace Brighid.Identity.Auth
{
    [Category("Unit")]
    public class SignupControllerTests
    {
        [Category("Unit")]
        public class Render
        {
            [Test]
            [Auto]
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

            [Test]
            [Auto]
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

            [Test]
            [Auto]
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

            [Test]
            [Auto]
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

        [Category("Unit")]
        public class Signup
        {
            [Test]
            [Auto]
            public async Task ShouldRender_IfModelStateIsInvalid(
                string password,
                string destination,
                SignupRequest request,
                [Frozen, Substitute] SignInManager<User> signInManager,
                [Target] SignupController signupController
            )
            {
                request.Password = password;
                request.ConfirmPassword = password;
                request.RedirectUri = new Uri(destination, UriKind.Relative);

                signupController.ModelState.AddModelError("key", "Invalid Model State");
                signInManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);

                var result = await signupController.Signup(request) as ViewResult;

                result!.Should().NotBeNull();
                result!.ViewName.Should().Be("~/Auth/Views/Signup.cshtml");
                result!.Model.As<SignupRequest>().RedirectUri.Should().Be(destination);
            }

            [Test]
            [Auto]
            public async Task ShouldAddModelErrors_IfUserCreationFails(
                string password,
                string message1,
                string message2,
                string destination,
                SignupRequest request,
                [Frozen, Substitute] SignInManager<User> signInManager,
                [Frozen, Substitute] IUserService userService,
                [Target] SignupController signupController
            )
            {
                request.Password = password;
                request.ConfirmPassword = password;
                request.RedirectUri = new Uri(destination, UriKind.Relative);

                var exceptions = new CreateUserException[] { new(message1), new(message2) };
                signInManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);
                userService.Create(Any<string>(), Any<string>()).Returns<User>(x => throw new CreateUserException(exceptions));

                var result = await signupController.Signup(request) as ViewResult;
                var errors = signupController.ModelState["signupError"]!.Errors;

                await userService.Received().Create(Is(request.Email), Is(request.Password));
                result!.Should().NotBeNull();
                errors.Should().Contain(error => error.ErrorMessage == message1);
                errors.Should().Contain(error => error.ErrorMessage == message2);
            }

            [Test]
            [Auto]
            public async Task ShouldNotIncludeDuplicateUserNameModelError(
                string email,
                string password,
                string destination,
                Exception randomErrorException,
                SignupRequest request,
                [Frozen, Substitute] SignInManager<User> signInManager,
                [Frozen, Substitute] IUserService userService,
                [Target] SignupController signupController
            )
            {
                request.Email = email;
                request.Password = password;
                request.ConfirmPassword = password;
                request.RedirectUri = new Uri(destination, UriKind.Relative);

                var duplicateUserNameMessage = new IdentityErrorDescriber().DuplicateUserName(email).Description;
                var duplicateUserNameException = new Exception(duplicateUserNameMessage);

                userService
                .When(svc => svc.Create(Any<string>(), Any<string>()))
                .Do(svc =>
                {
                    var exceptions = new[] { randomErrorException, duplicateUserNameException };
                    throw new AggregateException(exceptions);
                });

                signInManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);

                _ = await signupController.Signup(request) as ViewResult;

                signupController.ModelState["signupError"]!.Errors.Should().Contain(err =>
                    err.ErrorMessage == randomErrorException.Message
                );

                signupController.ModelState["signupError"]!.Errors.Should().NotContain(err =>
                    err.ErrorMessage == duplicateUserNameMessage
                );
            }

            [Test]
            [Auto]
            public async Task ShouldAddModelError_IfPasswordsDontMatch(
                string password1,
                string password2,
                string destination,
                SignupRequest request,
                User user,
                [Frozen, Substitute] SignInManager<User> signInManager,
                [Frozen, Substitute] IUserService userService,
                [Target] SignupController signupController
            )
            {
                request.Password = password1;
                request.ConfirmPassword = password2;
                request.RedirectUri = new Uri(destination, UriKind.Relative);

                signInManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);
                userService.Create(Any<string>(), Any<string>()).Returns(user);

                var result = await signupController.Signup(request) as ViewResult;
                var errors = signupController.ModelState["signupError"]!.Errors;

                result!.Should().NotBeNull();
                errors.Should().Contain(error => error.ErrorMessage == "Passwords do not match.");
            }

            [Test]
            [Auto]
            public async Task ShouldAddModelError_IfUserSigninFails(
                string password,
                string destination,
                SignupRequest request,
                User user,
                HttpContext httpContext,
                [Frozen, Substitute] SignInManager<User> signInManager,
                [Frozen, Substitute] IAuthService authService,
                [Frozen, Substitute] IUserService userService,
                [Target] SignupController signupController
            )
            {
                request.Password = password;
                request.ConfirmPassword = password;
                request.RedirectUri = new Uri(destination, UriKind.Relative);

                signInManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);
                authService.PasswordExchange(Any<string>(), Any<string>(), Any<Uri>(), Any<HttpContext>(), Any<CancellationToken>()).Throws(new InvalidCredentialsException(string.Empty));
                userService.Create(Any<string>(), Any<string>()).Returns(user);

                signupController.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var result = await signupController.Signup(request) as ViewResult;
                var errors = signupController.ModelState["signupError"]!.Errors;

                await authService.Received().PasswordExchange(Is(request.Email), Is(password), Is(request.RedirectUri), Is(httpContext), Is(httpContext.RequestAborted));
                result!.Should().NotBeNull();
                errors.Should().Contain(error => error.ErrorMessage == "Unable to sign in.");
            }

            [Test]
            [Auto]
            public async Task ShouldRedirectToDestination_IfSignupSucceeds(
                string password,
                string destination,
                SignupRequest request,
                User user,
                HttpContext httpContext,
                [Frozen] AuthenticationTicket ticket,
                [Frozen, Substitute] SignInManager<User> signInManager,
                [Frozen, Substitute] IUserService userService,
                [Target] SignupController signupController
            )
            {
                request.Password = password;
                request.ConfirmPassword = password;
                request.RedirectUri = new Uri(destination, UriKind.Relative);
                ticket.Properties.RedirectUri = destination;

                signInManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);
                userService.Create(Any<string>(), Any<string>()).Returns(user);

                signupController.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var result = await signupController.Signup(request) as SignInResult;
                result!.Should().NotBeNull();
                result!.Properties!.RedirectUri.Should().Be(destination);
            }
        }
    }
}
