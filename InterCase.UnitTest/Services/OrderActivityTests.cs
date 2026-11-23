using InterCase.Common;
using InterCase.Common.Dtos.Paging;
using InterCase.Repository.Entities;
using InterCase.UnitTest.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace InterCase.UnitTest.Services
{
    [TestClass]
    public sealed class OrderActivityTests
    {
        [TestMethod]
        public async Task GetLogsByUserIdAsync_ReadsOnlyProcessedOrdersOfTheRequestedPage()
        {
            var context = new OrderServiceContext();
            context.Orders.GetPageByUserIdAsync(context.User.PublicId, OrderStatus.Processed, 2, 5, Arg.Any<CancellationToken>())
                .Returns((new List<Order>(), 7));

            var result = await context.CreateService().GetLogsByUserIdAsync(context.User.PublicId, new PageRequest { Page = 2, PageSize = 5 });

            Assert.IsEmpty(result.Items);
            Assert.AreEqual(2, result.Page);
            Assert.AreEqual(5, result.PageSize);
            Assert.AreEqual(7, result.TotalCount);
            Assert.AreEqual(2, result.TotalPages);
            await context.Orders.Received(1).GetPageByUserIdAsync(context.User.PublicId, OrderStatus.Processed, 2, 5, Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task GetLogsByUserIdAsync_AddsTheRedisLogOfEachOrder()
        {
            var context = new OrderServiceContext();
            var logged = context.CreateOrder(OrderStatus.Processed);
            var missing = context.CreateOrder(OrderStatus.Processed);
            context.Orders.GetPageByUserIdAsync(context.User.PublicId, OrderStatus.Processed, 1, 10, Arg.Any<CancellationToken>())
                .Returns((new List<Order> { logged, missing }, 2));
            context.Cache.GetStringAsync(CacheKeys.OrderLog(logged.Id)).Returns($"Order {logged.Id} processed at 2026-11-23T12:00:00Z");
            context.Cache.GetStringAsync(CacheKeys.OrderLog(missing.Id)).Returns((string?)null);

            var result = await context.CreateService().GetLogsByUserIdAsync(context.User.PublicId, new PageRequest());

            Assert.HasCount(2, result.Items);
            Assert.AreEqual(logged.PublicId, result.Items[0].OrderId);
            Assert.AreEqual($"Order {logged.Id} processed at 2026-11-23T12:00:00Z", result.Items[0].Log);
            Assert.AreEqual(OrderStatus.Processed, result.Items[0].Status);
            Assert.AreEqual(logged.ProcessedAt, result.Items[0].ProcessedAt);
            Assert.AreEqual(missing.PublicId, result.Items[1].OrderId);
            Assert.IsNull(result.Items[1].Log);
        }

        [TestMethod]
        public async Task RepublishStalePendingAsync_PublishesEveryOrderPendingLongerThanConfigured()
        {
            var context = new OrderServiceContext();
            var first = context.CreateOrder();
            var second = context.CreateOrder();
            context.Orders.GetStalePendingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns([first, second]);
            var cutoff = context.Time.GetUtcNow().UtcDateTime.AddMinutes(-new ProcessingOptions().StalePendingMinutes);

            var published = await context.CreateService().RepublishStalePendingAsync();

            Assert.AreEqual(2, published);
            await context.Orders.Received(1).GetStalePendingAsync(cutoff, Arg.Any<CancellationToken>());
            await context.Queue.Received(1).PublishAsync(QueueNames.OrderPlaced, Arg.Is<OrderPlacedMessage>(m => m.OrderId == first.Id), OrderServiceContext.CorrelationId, Arg.Any<CancellationToken>());
            await context.Queue.Received(1).PublishAsync(QueueNames.OrderPlaced, Arg.Is<OrderPlacedMessage>(m => m.OrderId == second.Id), OrderServiceContext.CorrelationId, Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task RepublishStalePendingAsync_WhenOnePublishFails_ContinuesAndCountsOnlySuccesses()
        {
            var context = new OrderServiceContext();
            var failing = context.CreateOrder();
            var succeeding = context.CreateOrder();
            context.Orders.GetStalePendingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns([failing, succeeding]);
            context.Queue.PublishAsync(QueueNames.OrderPlaced, Arg.Is<OrderPlacedMessage>(m => m.OrderId == failing.Id), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("broker down"));

            var published = await context.CreateService().RepublishStalePendingAsync();

            Assert.AreEqual(1, published);
            await context.Queue.Received(1).PublishAsync(QueueNames.OrderPlaced, Arg.Is<OrderPlacedMessage>(m => m.OrderId == succeeding.Id), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task RepublishStalePendingAsync_WhenNothingIsStale_PublishesNothing()
        {
            var context = new OrderServiceContext();
            context.Orders.GetStalePendingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(new List<Order>());

            var published = await context.CreateService().RepublishStalePendingAsync();

            Assert.AreEqual(0, published);
            await context.Queue.DidNotReceiveWithAnyArgs().PublishAsync<OrderPlacedMessage>(default!, default!, default, default);
        }
    }
}
