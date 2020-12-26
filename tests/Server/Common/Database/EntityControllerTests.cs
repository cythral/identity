using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Identity.Sns;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

#pragma warning disable CA1040

namespace Brighid.Identity
{

    public class Item { public Guid Id { get; set; } }

    public interface IItemRepository : IRepository<Item, Guid> { }

    public interface IItemService : IEntityService<Item, Guid> { }

    public class ItemController : EntityController<Item, Guid, IItemRepository, IItemService>
    {
        public const string BasePath = "/api/items";

        public ItemController(
            IItemService service,
            IItemRepository repository
        ) : base(BasePath, service, repository)
        {
        }

    }

    [TestFixture, Category("Unit")]
    public class ApplicationControllerTests
    {
        public static HttpContext SetupHttpContext(Controller controller, IdentityRequestSource source = IdentityRequestSource.Direct)
        {
            var itemDictionary = new Dictionary<object, object?>();
            var httpContext = Substitute.For<HttpContext>();
            var controllerContext = new ControllerContext { HttpContext = httpContext };
            controller.ControllerContext = controllerContext;
            httpContext.Items.Returns(itemDictionary);
            httpContext.Items[Constants.RequestSource] = source;
            return httpContext;
        }

        [TestFixture, Category("Unit")]
        public class CreateTests
        {
            [Test, Auto]
            public async Task ShouldCreateAndReturnItem(
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                itemService.Create(Any<Item>()).Returns(item);
                SetupHttpContext(controller);

                var response = await controller.Create(item);
                var result = response.Result;

                result.As<CreatedResult>().Value.Should().Be(item);
                await itemService.Received().Create(Is(item));
            }

            [Test, Auto]
            public async Task ShouldRedirectToApplicationPage(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.Create(Any<Item>()).Returns(item);
                SetupHttpContext(controller);

                var response = await controller.Create(item);
                var result = response.Result;

                result.As<CreatedResult>().Location.Should().Be($"/api/items/{id}");
            }

            [Test, Auto]
            public async Task ShouldSetIdItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.Create(Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.Create(item);

                httpContext.Items[CloudFormationConstants.Id].Should().Be(id);
            }

            [Test, Auto]
            public async Task ShouldNotSetIdItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.Create(Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.Create(item);

                httpContext.Items.Should().NotContainKey(CloudFormationConstants.Id);
            }

            [Test, Auto]
            public async Task ShouldSetDataItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                Item request,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.Create(Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.Create(request);

                httpContext.Items[CloudFormationConstants.Data].Should().Be(item);
            }

            [Test, Auto]
            public async Task ShouldNotSetDataItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.Create(Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.Create(item);

                httpContext.Items.Should().NotContainKey(CloudFormationConstants.Data);
            }
        }

        [TestFixture, Category("Unit")]
        public class GetByIdTests
        {
            [Test, Auto]
            public async Task ShouldReturnEntityIfItExists(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemRepository repository,
                [Target] ItemController controller
            )
            {
                repository.FindById(Any<Guid>()).Returns(item);
                SetupHttpContext(controller);

                var response = await controller.GetById(id);
                var result = response.Result;

                result.As<OkObjectResult>().Value.Should().Be(item);
                await repository.Received().FindById(Is(id));
            }

            [Test, Auto]
            public async Task ShouldReturnNotFoundIfNotExists(
                Guid id,
                [Frozen, Substitute] IItemRepository repository,
                [Target] ItemController controller
            )
            {
                repository.FindById(Any<Guid>()).Returns((Item)null!);
                SetupHttpContext(controller);

                var response = await controller.GetById(id);
                var result = response.Result;

                result.Should().BeOfType<NotFoundResult>();
            }
        }

        [TestFixture, Category("Unit")]
        public class UpdateByIdTests
        {
            [Test, Auto]
            public async Task ShouldUpdateAndReturnApplication(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.UpdateById(Any<Guid>(), Any<Item>()).Returns(item);
                SetupHttpContext(controller);

                var response = await controller.UpdateById(id, item);
                var result = response.Result;

                result.As<OkObjectResult>().Value.Should().Be(item);
                await itemService.Received().UpdateById(Is(id), Is(item));
            }

            [Test, Auto]
            public async Task ShouldSetIdItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.UpdateById(Any<Guid>(), Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.UpdateById(id, item);

                httpContext.Items[CloudFormationConstants.Id].Should().Be(id);
            }

            [Test, Auto]
            public async Task ShouldNotSetIdItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.UpdateById(Any<Guid>(), Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.UpdateById(id, item);

                httpContext.Items.Should().NotContainKey(CloudFormationConstants.Id);
            }

            [Test, Auto]
            public async Task ShouldSetDataItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.UpdateById(Any<Guid>(), Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.UpdateById(id, item);

                httpContext.Items[CloudFormationConstants.Data].Should().Be(item);
            }

            [Test, Auto]
            public async Task ShouldNotSetDataItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.UpdateById(Any<Guid>(), Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.UpdateById(id, item);

                httpContext.Items.Should().NotContainKey(CloudFormationConstants.Data);
            }
        }

        [TestFixture, Category("Unit")]
        public class DeleteByIdTests
        {
            [Test, Auto]
            public async Task ShouldUpdateAndReturnApplication(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.DeleteById(Any<Guid>()).Returns(item);
                SetupHttpContext(controller);

                var response = await controller.DeleteById(id);
                var result = response.Result;

                result.As<OkObjectResult>().Value.Should().Be(item);
                await itemService.Received().DeleteById(Is(id));
            }

            [Test, Auto]
            public async Task ShouldSetIdItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.DeleteById(Any<Guid>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.DeleteById(id);

                httpContext.Items[CloudFormationConstants.Id].Should().Be(id);
            }

            [Test, Auto]
            public async Task ShouldNotSetIdItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.DeleteById(Any<Guid>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.DeleteById(id);

                httpContext.Items.Should().NotContainKey(CloudFormationConstants.Id);
            }

            [Test, Auto]
            public async Task ShouldSetDataItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.DeleteById(Any<Guid>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.DeleteById(id);

                httpContext.Items[CloudFormationConstants.Data].Should().Be(item);
            }

            [Test, Auto]
            public async Task ShouldNotSetDataItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Item item,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.DeleteById(Any<Guid>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.DeleteById(id);

                httpContext.Items.Should().NotContainKey(CloudFormationConstants.Data);
            }
        }
    }
}
