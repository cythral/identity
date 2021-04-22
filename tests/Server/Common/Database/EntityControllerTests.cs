using System;
using System.Collections.Generic;
using System.Threading;
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
    public class ItemRequest { public string Name { get; set; } }

    public interface IItemRepository : IRepository<Item, Guid> { }
    public interface IItemMapper : IRequestToEntityMapper<ItemRequest, Item> { }

    public interface IItemService : IEntityService<Item, Guid> { }

    public class ItemController : EntityController<Item, ItemRequest, Guid, IItemRepository, IItemMapper, IItemService>
    {
        public const string BasePath = "/api/items";

        public ItemController(
            IItemMapper mapper,
            IItemService service,
            IItemRepository repository
        ) : base(BasePath, mapper, service, repository)
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
            public async Task ShouldMapCreateAndReturnItem(
                ItemRequest request,
                Item item,
                [Frozen, Substitute] IItemMapper itemMapper,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                itemMapper.MapRequestToEntity(Any<ItemRequest>(), Any<CancellationToken>()).Returns(item);
                itemService.Create(Any<Item>()).Returns(item);
                SetupHttpContext(controller);

                var response = await controller.Create(request);
                var result = response.Result;

                result.As<CreatedResult>().Value.Should().Be(item);
                await itemMapper.Received().MapRequestToEntity(Is(request), Any<CancellationToken>());
                await itemService.Received().Create(Is(item));
            }

            [Test, Auto]
            public async Task ShouldRedirectToApplicationPage(
                Guid id,
                ItemRequest request,
                Item item,
                [Frozen, Substitute] IItemMapper itemMapper,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemMapper.MapRequestToEntity(Any<ItemRequest>(), Any<CancellationToken>()).Returns(item);
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.Create(Any<Item>()).Returns(item);
                SetupHttpContext(controller);

                var response = await controller.Create(request);
                var result = response.Result;

                result.As<CreatedResult>().Location.Should().Be($"/api/items/{id}");
            }

            [Test, Auto]
            public async Task ShouldSetIdItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                ItemRequest request,
                Item item,
                [Frozen, Substitute] IItemMapper itemMapper,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemMapper.MapRequestToEntity(Any<ItemRequest>(), Any<CancellationToken>()).Returns(item);
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.Create(Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.Create(request);

                httpContext.Items[CloudFormationConstants.Id].Should().Be(id);
            }

            [Test, Auto]
            public async Task ShouldNotSetIdItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                ItemRequest request,
                Item item,
                [Frozen, Substitute] IItemMapper itemMapper,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemMapper.MapRequestToEntity(Any<ItemRequest>(), Any<CancellationToken>()).Returns(item);
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.Create(Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.Create(request);

                httpContext.Items.Should().NotContainKey(CloudFormationConstants.Id);
            }

            [Test, Auto]
            public async Task ShouldSetDataItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                ItemRequest request,
                Item entity,
                Item resultingItem,
                [Frozen, Substitute] IItemMapper itemMapper,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                resultingItem.Id = id;
                itemMapper.MapRequestToEntity(Any<ItemRequest>(), Any<CancellationToken>()).Returns(entity);
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.Create(Any<Item>()).Returns(resultingItem);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.Create(request);

                httpContext.Items[CloudFormationConstants.Data].Should().Be(resultingItem);
            }

            [Test, Auto]
            public async Task ShouldNotSetDataItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Item item,
                ItemRequest request,
                [Frozen, Substitute] IItemMapper itemMapper,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemMapper.MapRequestToEntity(Any<ItemRequest>(), Any<CancellationToken>()).Returns(item);
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.Create(Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.Create(request);

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
            public async Task ShouldMapUpdateAndReturnApplication(
                Guid id,
                Item item,
                ItemRequest request,
                [Frozen, Substitute] IItemMapper itemMapper,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                itemMapper.MapRequestToEntity(Any<ItemRequest>(), Any<CancellationToken>()).Returns(item);
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.UpdateById(Any<Guid>(), Any<Item>()).Returns(item);
                SetupHttpContext(controller);

                var response = await controller.UpdateById(id, request);
                var result = response.Result;

                result.As<OkObjectResult>().Value.Should().Be(item);
                await itemMapper.Received().MapRequestToEntity(Is(request), Any<CancellationToken>());
                await itemService.Received().UpdateById(Is(id), Is(item));
            }

            [Test, Auto]
            public async Task ShouldSetIdItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                Item item,
                ItemRequest request,
                [Frozen, Substitute] IItemMapper itemMapper,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                itemMapper.MapRequestToEntity(Any<ItemRequest>(), Any<CancellationToken>()).Returns(item);
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.UpdateById(Any<Guid>(), Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.UpdateById(id, request);

                httpContext.Items[CloudFormationConstants.Id].Should().Be(id);
            }

            [Test, Auto]
            public async Task ShouldNotSetIdItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Item item,
                ItemRequest request,
                [Frozen, Substitute] IItemMapper itemMapper,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                itemMapper.MapRequestToEntity(Any<ItemRequest>(), Any<CancellationToken>()).Returns(item);
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.UpdateById(Any<Guid>(), Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.UpdateById(id, request);

                httpContext.Items.Should().NotContainKey(CloudFormationConstants.Id);
            }

            [Test, Auto]
            public async Task ShouldSetDataItemInHttpContext_IfRequestSourceIsSns(
                Guid id,
                Item item,
                ItemRequest request,
                [Frozen, Substitute] IItemMapper itemMapper,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemMapper.MapRequestToEntity(Any<ItemRequest>(), Any<CancellationToken>()).Returns(item);
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.UpdateById(Any<Guid>(), Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Sns);

                await controller.UpdateById(id, request);

                httpContext.Items[CloudFormationConstants.Data].Should().Be(item);
            }

            [Test, Auto]
            public async Task ShouldNotSetDataItemInHttpContext_IfRequestSourceIsDirect(
                Guid id,
                Item item,
                ItemRequest request,
                [Frozen, Substitute] IItemMapper itemMapper,
                [Frozen, Substitute] IItemService itemService,
                [Target] ItemController controller
            )
            {
                item.Id = id;
                itemMapper.MapRequestToEntity(Any<ItemRequest>(), Any<CancellationToken>()).Returns(item);
                itemService.GetPrimaryKey(Any<Item>()).Returns(id);
                itemService.UpdateById(Any<Guid>(), Any<Item>()).Returns(item);
                var httpContext = SetupHttpContext(controller, IdentityRequestSource.Direct);

                await controller.UpdateById(id, request);

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
