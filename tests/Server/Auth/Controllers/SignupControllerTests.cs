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

        public class Signup
        {
            [Test, Auto]
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

            [Test, Auto]
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
                var errors = signupController.ModelState["signupError"].Errors;

                await userService.Received().Create(Is(request.Email), Is(request.Password));
                result!.Should().NotBeNull();
                errors.Should().Contain(error => error.ErrorMessage == message1);
                errors.Should().Contain(error => error.ErrorMessage == message2);
            }

            [Test, Auto]
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
                signInManager.PasswordSignInAsync(Any<string>(), Any<string>(), Any<bool>(), Any<bool>()).Returns(SignInResult.Success);
                userService.Create(Any<string>(), Any<string>()).Returns(user);

                var result = await signupController.Signup(request) as ViewResult;
                var errors = signupController.ModelState["signupError"].Errors;

                result!.Should().NotBeNull();
                errors.Should().Contain(error => error.ErrorMessage == "Passwords do not match.");
            }


            [Test, Auto]
            public async Task ShouldAddModelError_IfUserSigninFails(
                string password,
                string destination,
                SignupRequest request,
                User user,
                [Frozen, Substitute] SignInManager<User> signInManager,
                [Frozen, Substitute] IUserService userService,
                [Target] SignupController signupController
            )
            {
                request.Password = password;
                request.ConfirmPassword = password;
                request.RedirectUri = new Uri(destination, UriKind.Relative);

                signInManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);
                signInManager.PasswordSignInAsync(Any<string>(), Any<string>(), Any<bool>(), Any<bool>()).Returns(SignInResult.Failed);
                userService.Create(Any<string>(), Any<string>()).Returns(user);

                var result = await signupController.Signup(request) as ViewResult;
                var errors = signupController.ModelState["signupError"].Errors;

                await signInManager.Received().PasswordSignInAsync(Is(user), Is(request.Password), Is(false), Is(false));
                result!.Should().NotBeNull();
                errors.Should().Contain(error => error.ErrorMessage == "Unable to sign in.");
            }

            [Test, Auto]
            public async Task ShouldRedirectToDestination_IfSignupSucceeds(
                string password,
                string destination,
                SignupRequest request,
                User user,
                [Frozen, Substitute] SignInManager<User> signInManager,
                [Frozen, Substitute] IUserService userService,
                [Target] SignupController signupController
            )
            {
                request.Password = password;
                request.ConfirmPassword = password;
                request.RedirectUri = new Uri(destination, UriKind.Relative);

                signInManager.IsSignedIn(Any<ClaimsPrincipal>()).Returns(false);
                signInManager.PasswordSignInAsync(Any<User>(), Any<string>(), Any<bool>(), Any<bool>()).Returns(SignInResult.Success);
                userService.Create(Any<string>(), Any<string>()).Returns(user);

                var result = await signupController.Signup(request) as LocalRedirectResult;
                result!.Should().NotBeNull();
                result!.Url.Should().Be(destination);
            }
        }
    }
}
