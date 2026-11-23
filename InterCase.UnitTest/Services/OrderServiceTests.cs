using InterCase.Common;
using InterCase.Common.Dtos.Orders;
using InterCase.Common.Helpers;
using InterCase.Repository.Entities;
using InterCase.Service.Interfaces;
using InterCase.Service.Services.OtherServices;
using InterCase.UnitTest.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace InterCase.UnitTest.Services
{
    [TestClass]
    public sealed class OrderServiceTests
    {
        private static CreateOrderRequest RequestFor(Guid userId) => new()
        {
            UserId = userId,
            ProductId = Guid.NewGuid(),
            Quantity = 3,
            PaymentMethod = PaymentMethod.BankTransfer
        };

        [TestMethod]
        public async Task CreateAsync_WhenRequestIsValid_SavesPendingOrderOnce()
        {
            var context = new OrderServiceContext();
            var request = RequestFor(context.User.PublicId);

            await context.CreateService().CreateAsync(request);

            var order = Assert.ContainsSingle(context.AddedOrders);
            Assert.AreEqual(OrderStatus.Pending, order.Status);
            Assert.AreEqual(request.ProductId, order.ProductId);
            Assert.AreEqual(request.Quantity, order.Quantity);
            Assert.AreSame(context.User, order.User);
            await context.Repositories.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task CreateAsync_WhenRequestIsValid_PublishesOrderPlacedMessage()
        {
            var context = new OrderServiceContext();

            await context.CreateService().CreateAsync(RequestFor(context.User.PublicId));

            var order = Assert.ContainsSingle(context.AddedOrders);
            await context.Queue.Received(1).PublishAsync(
                QueueNames.OrderPlaced,
                Arg.Is<OrderPlacedMessage>(m =>
                    m.OrderId == order.Id &&
                    m.UserId == context.User.PublicId &&
                    m.CorrelationId == OrderServiceContext.CorrelationId),
                OrderServiceContext.CorrelationId,
                Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task CreateAsync_WhenRequestIsValid_RemovesUserOrdersFromCache()
        {
            var context = new OrderServiceContext();

            await context.CreateService().CreateAsync(RequestFor(context.User.PublicId));

            await context.Cache.Received(1).RemoveAsync(CacheKeys.OrdersByUser(context.User.PublicId));
        }

        [TestMethod]
        public async Task CreateAsync_ReturnsPublicIdAsOrderId()
        {
            var context = new OrderServiceContext();

            var response = await context.CreateService().CreateAsync(RequestFor(context.User.PublicId));

            var order = Assert.ContainsSingle(context.AddedOrders);
            Assert.AreNotEqual(Guid.Empty, response.Id);
            Assert.AreEqual(order.PublicId, response.Id);
            Assert.AreEqual(context.User.PublicId, response.UserId);
        }

        [TestMethod]
        public void OrderResponse_DoesNotExposeTheDatabaseId()
        {
            var longProperties = typeof(OrderResponse).GetProperties().Where(p => p.PropertyType == typeof(long));

            Assert.IsEmpty(longProperties);
        }

        [TestMethod]
        public async Task CreateAsync_WhenUserDoesNotExist_ThrowsAndNeitherSavesNorPublishes()
        {
            var context = new OrderServiceContext();
            var unknownUserId = Guid.NewGuid();
            context.Users.GetByPublicIdAsync(unknownUserId, true, Arg.Any<CancellationToken>()).Returns((User?)null);

            await Assert.ThrowsExactlyAsync<NotFoundException>(() => context.CreateService().CreateAsync(RequestFor(unknownUserId)));

            Assert.IsEmpty(context.AddedOrders);
            await context.Repositories.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
            await context.Queue.DidNotReceiveWithAnyArgs().PublishAsync<OrderPlacedMessage>(default!, default!, default, default);
        }

        [TestMethod]
        public async Task CreateAsync_WhenPublishFails_StillReturnsTheOrderAndClearsCache()
        {
            var context = new OrderServiceContext();
            context.Queue
                .PublishAsync(Arg.Any<string>(), Arg.Any<OrderPlacedMessage>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("broker down"));

            var response = await context.CreateService().CreateAsync(RequestFor(context.User.PublicId));

            Assert.AreEqual(Assert.ContainsSingle(context.AddedOrders).PublicId, response.Id);
            await context.Cache.Received(1).RemoveAsync(CacheKeys.OrdersByUser(context.User.PublicId));
        }

        [TestMethod]
        public async Task GetByUserIdAsync_WhenCacheHasOrders_DoesNotQueryRepository()
        {
            var context = new OrderServiceContext();
            var key = CacheKeys.OrdersByUser(context.User.PublicId);
            var cached = new List<OrderResponse> { OrderServiceContext.Mapper.Map<OrderResponse>(context.CreateOrder()) };
            context.Cache.GetAsync<List<OrderResponse>>(key).Returns(cached);

            var result = await context.CreateService().GetByUserIdAsync(context.User.PublicId);

            Assert.AreSame(cached, result);
            await context.Orders.DidNotReceiveWithAnyArgs().GetByUserIdAsync(default, default);
        }

        [TestMethod]
        public async Task GetByUserIdAsync_WhenCacheIsEmpty_QueriesRepositoryAndCachesWithConfiguredTtl()
        {
            var context = new OrderServiceContext();
            var key = CacheKeys.OrdersByUser(context.User.PublicId);
            var order = context.CreateOrder();
            context.Cache.GetAsync<List<OrderResponse>>(key).Returns((List<OrderResponse>?)null);
            context.Orders.GetByUserIdAsync(context.User.PublicId, Arg.Any<CancellationToken>()).Returns([order]);

            var result = await context.CreateService().GetByUserIdAsync(context.User.PublicId);

            Assert.AreEqual(order.PublicId, Assert.ContainsSingle(result).Id);
            await context.Cache.Received(1).SetAsync(
                key,
                Arg.Is<List<OrderResponse>>(list => list.Count == 1 && list[0].Id == order.PublicId),
                TimeSpan.FromSeconds(OrderServiceContext.OrdersTtlSeconds));
        }

        [TestMethod]
        public async Task GetByUserIdAsync_WhenRedisIsUnavailable_ReturnsOrdersFromDatabase()
        {
            var context = new OrderServiceContext();
            var order = context.CreateOrder();
            context.Orders.GetByUserIdAsync(context.User.PublicId, Arg.Any<CancellationToken>()).Returns([order]);

            var unavailable = new CacheUnavailableException("redis down", new TimeoutException());
            var manager = Substitute.For<ICacheManager>();
            manager.GetAsync(Arg.Any<string>()).ThrowsAsync(unavailable);
            manager.SetAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<TimeSpan?>()).ThrowsAsync(unavailable);
            var cache = new CacheService(manager, NullLogger<CacheService>.Instance);

            var result = await context.CreateService(cache).GetByUserIdAsync(context.User.PublicId);

            Assert.AreEqual(order.PublicId, Assert.ContainsSingle(result).Id);
        }

        [TestMethod]
        public async Task UpdateAsync_WhenOrderDoesNotExist_ThrowsNotFoundException()
        {
            var context = new OrderServiceContext();
            var request = new UpdateOrderRequest { ProductId = Guid.NewGuid(), Quantity = 1, PaymentMethod = PaymentMethod.Cash };

            await Assert.ThrowsExactlyAsync<NotFoundException>(() => context.CreateService().UpdateAsync(Guid.NewGuid(), request));

            await context.Repositories.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task DeleteAsync_WhenOrderDoesNotExist_ThrowsNotFoundException()
        {
            var context = new OrderServiceContext();

            await Assert.ThrowsExactlyAsync<NotFoundException>(() => context.CreateService().DeleteAsync(Guid.NewGuid()));

            await context.Repositories.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [TestMethod]
        [DataRow(OrderStatus.Processing)]
        [DataRow(OrderStatus.Processed)]
        [DataRow(OrderStatus.Failed)]
        public async Task UpdateAsync_WhenOrderIsNotPending_ThrowsConflictException(OrderStatus status)
        {
            var context = new OrderServiceContext();
            var order = context.StoreOrder(status);
            var request = new UpdateOrderRequest { ProductId = Guid.NewGuid(), Quantity = 1, PaymentMethod = PaymentMethod.Cash };

            var exception = await Assert.ThrowsExactlyAsync<ConflictException>(() => context.CreateService().UpdateAsync(order.PublicId, request));

            Assert.AreEqual(Localization.Keys.OrderNotPending, exception.MessageKey);
            await context.Repositories.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task UpdateAsync_WhenOrderIsPending_AppliesChangesAndClearsCache()
        {
            var context = new OrderServiceContext();
            var order = context.StoreOrder();
            var request = new UpdateOrderRequest { ProductId = Guid.NewGuid(), Quantity = 9, PaymentMethod = PaymentMethod.CreditCard };

            var response = await context.CreateService().UpdateAsync(order.PublicId, request);

            Assert.AreEqual(9, response.Quantity);
            Assert.AreEqual(request.ProductId, order.ProductId);
            Assert.AreEqual(PaymentMethod.CreditCard, order.PaymentMethod);
            Assert.AreEqual(OrderStatus.Pending, order.Status);
            await context.Repositories.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await context.Cache.Received(1).RemoveAsync(CacheKeys.OrdersByUser(context.User.PublicId));
        }

        [TestMethod]
        public async Task DeleteAsync_WhenOrderIsPending_DeletesAndClearsCache()
        {
            var context = new OrderServiceContext();
            var order = context.StoreOrder();

            await context.CreateService().DeleteAsync(order.PublicId);

            context.Orders.Received(1).Delete(order);
            await context.Repositories.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await context.Cache.Received(1).RemoveAsync(CacheKeys.OrdersByUser(context.User.PublicId));
        }
    }
}
