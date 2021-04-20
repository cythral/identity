using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Users;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Identity.LoginProviders
{
    [Category("Unit")]
    public class LoginProviderControllerTests
    {

        [Category("Unit")]
        public class GetUserByLoginProviderTests
        {
            [Test, Auto]
            public async Task ShouldReturnNotFound_IfUserDoesntExist(
                string loginProvider,
                string providerKey,
                [Frozen, Substitute] IUserRepository repository,
                [Target] LoginProviderController controller
            )
            {
                repository.FindByLogin(Any<string>(), Any<string>(), Any<string[]>()).Returns((User)null!);

                var response = await controller.GetUserByLoginProvider(loginProvider, providerKey);
                var result = response.Result;

                result.Should().BeOfType<NotFoundResult>();
                await repository.Received().FindByLogin(Is(loginProvider), Is(providerKey), Any<string[]>());
            }

            [Test, Auto]
            public async Task ShouldReturnOk_IfUserExists(
                string loginProvider,
                string providerKey,
                User user,
                [Frozen, Substitute] IUserRepository repository,
                [Target] LoginProviderController controller
            )
            {
                repository.FindByLogin(Any<string>(), Any<string>(), Any<string[]>()).Returns(user);

                var response = await controller.GetUserByLoginProvider(loginProvider, providerKey);
                var result = response.Result;

                result.Should().BeOfType<OkObjectResult>();
                result.As<OkObjectResult>().Value.Should().Be(user);
                await repository.Received().FindByLogin(Is(loginProvider), Is(providerKey), Any<string[]>());
            }
        }
    }
}
