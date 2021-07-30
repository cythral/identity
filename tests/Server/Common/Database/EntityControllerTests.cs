using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

#pragma warning disable CA1040, SA1649, SA1649, SA1402

namespace Brighid.Identity
{
    public interface IItemRepository : IRepository<Item, Guid>
    {
    }

    public interface IItemMapper : IRequestToEntityMapper<ItemRequest, Item>
    {
    }

    public interface IItemService : IEntityService<Item, Guid>
    {
    }

    public class Item
    {
        public Guid Id { get; set; }
    }

    public class ItemRequest
    {
        public string Name { get; set; }
    }

    public class ItemController : EntityController<Item, ItemRequest, Guid, IItemRepository, IItemMapper, IItemService>
    {
        public const string BasePath = "/api/items";

        public ItemController(
            IItemMapper mapper,
            IItemService service,
            IItemRepository repository
        )
            : base(BasePath, mapper, service, repository)
        {
        }
    }

    [TestFixture]
    [Category("Unit")]
    public class ApplicationControllerTests
    {
        public static HttpContext SetupHttpContext(Controller controller)
        {
            var itemDictionary = new Dictionary<object, object?>();
            var httpContext = Substitute.For<HttpContext>();
            var controllerContext = new ControllerContext { HttpContext = httpContext };
            controller.ControllerContext = controllerContext;
            httpContext.Items.Returns(itemDictionary);
            return httpContext;
        }

        [TestFixture]
        [Category("Unit")]
        public class CreateTests
        {
            [Test]
            [Auto]
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

            [Test]
            [Auto]
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
        }

        [TestFixture]
        [Category("Unit")]
        public class GetByIdTests
        {
            [Test]
            [Auto]
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

            [Test]
            [Auto]
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

        [TestFixture]
        [Category("Unit")]
        public class UpdateByIdTests
        {
            [Test]
            [Auto]
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
        }

        [TestFixture]
        [Category("Unit")]
        public class DeleteByIdTests
        {
            [Test]
            [Auto]
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
        }
    }
}
