using InterCase.Common;
using InterCase.UnitTest.Helpers;
using NSubstitute;

namespace InterCase.UnitTest.Services
{
    [TestClass]
    public sealed class OrderProcessingTests
    {
        private static readonly TimeSpan ProcessingDelay = TimeSpan.FromSeconds(OrderServiceContext.ProcessingDelaySeconds);

        [TestMethod]
        public async Task ProcessAsync_WhenOrderDoesNotExist_CompletesWithoutSaving()
        {
            var context = new OrderServiceContext();

            await context.CreateService().ProcessAsync(404);

            await context.Repositories.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
            await context.Cache.DidNotReceiveWithAnyArgs().SetStringAsync(default!, default!);
        }

        [TestMethod]
        public async Task ProcessAsync_WhenOrderIsAlreadyProcessed_DoesNotProcessItAgain()
        {
            var context = new OrderServiceContext();
            var order = context.StoreOrder(OrderStatus.Processed);
            var processedAt = order.ProcessedAt;

            await context.CreateService().ProcessAsync(order.Id);

            Assert.AreEqual(OrderStatus.Processed, order.Status);
            Assert.AreEqual(processedAt, order.ProcessedAt);
            Assert.IsEmpty(order.Notifications);
            await context.Repositories.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task ProcessAsync_WhenOrderIsPending_MarksProcessedAndAddsNotification()
        {
            var context = new OrderServiceContext();
            var order = context.StoreOrder();
            var expectedProcessedAt = context.Time.GetUtcNow().UtcDateTime + ProcessingDelay;

            var processing = context.CreateService().ProcessAsync(order.Id);
            context.Time.Advance(ProcessingDelay);
            await processing;

            Assert.AreEqual(OrderStatus.Processed, order.Status);
            Assert.AreEqual(expectedProcessedAt, order.ProcessedAt);
            var notification = Assert.ContainsSingle(order.Notifications);
            Assert.AreEqual(NotificationChannels.Console, notification.Channel);
            Assert.AreEqual(order.UserId, notification.UserId);
            Assert.AreEqual(expectedProcessedAt, notification.SentAt);
            Assert.Contains(order.PublicId.ToString(), notification.Message);
            await context.Repositories.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task ProcessAsync_WhenOrderIsPending_WritesRedisLogAndClearsUserCache()
        {
            var context = new OrderServiceContext();
            var order = context.StoreOrder();

            var processing = context.CreateService().ProcessAsync(order.Id);
            context.Time.Advance(ProcessingDelay);
            await processing;

            await context.Cache.Received(1).SetStringAsync(
                CacheKeys.OrderLog(order.Id),
                Arg.Is<string>(log => log.Contains($"Order {order.Id} processed at")));
            await context.Cache.Received(1).RemoveAsync(CacheKeys.OrdersByUser(context.User.PublicId));
        }

        [TestMethod]
        public async Task ProcessAsync_WaitsForTheConfiguredDelayInProcessingState()
        {
            var context = new OrderServiceContext();
            var order = context.StoreOrder();

            var processing = context.CreateService().ProcessAsync(order.Id);
            context.Time.Advance(ProcessingDelay - TimeSpan.FromMilliseconds(1));

            Assert.IsFalse(processing.IsCompleted);
            Assert.AreEqual(OrderStatus.Processing, order.Status);

            context.Time.Advance(TimeSpan.FromMilliseconds(1));
            await processing;

            Assert.AreEqual(OrderStatus.Processed, order.Status);
        }

        [TestMethod]
        public async Task MarkFailedAsync_WhenOrderIsNotProcessed_SetsFailedAndClearsCache()
        {
            var context = new OrderServiceContext();
            var order = context.StoreOrder(OrderStatus.Processing);

            await context.CreateService().MarkFailedAsync(order.Id);

            Assert.AreEqual(OrderStatus.Failed, order.Status);
            await context.Repositories.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await context.Cache.Received(1).RemoveAsync(CacheKeys.OrdersByUser(context.User.PublicId));
        }

        [TestMethod]
        public async Task MarkFailedAsync_WhenOrderIsAlreadyProcessed_KeepsItProcessed()
        {
            var context = new OrderServiceContext();
            var order = context.StoreOrder(OrderStatus.Processed);

            await context.CreateService().MarkFailedAsync(order.Id);

            Assert.AreEqual(OrderStatus.Processed, order.Status);
            await context.Repositories.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
