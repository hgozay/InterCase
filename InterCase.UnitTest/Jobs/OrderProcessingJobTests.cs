using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using InterCase.Common;
using InterCase.Service.Interfaces;
using InterCase.Service.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace InterCase.UnitTest.Jobs
{
    [TestClass]
    public sealed class OrderProcessingJobTests
    {
        private const long OrderId = 42;

        private readonly IOrderService _orders = Substitute.For<IOrderService>();
        private readonly IOrderService _failureOrders = Substitute.For<IOrderService>();
        private readonly CorrelationContext _correlation = new();

        private OrderProcessingJob CreateJob()
        {
            var serviceManager = Substitute.For<IServiceManager>();
            serviceManager.Order.Returns(_orders);

            var failureManager = Substitute.For<IServiceManager>();
            failureManager.Order.Returns(_failureOrders);
            var scopeFactory = new ServiceCollection()
                .AddScoped(_ => failureManager)
                .BuildServiceProvider()
                .GetRequiredService<IServiceScopeFactory>();

            return new OrderProcessingJob(serviceManager, scopeFactory, _correlation, NullLogger<OrderProcessingJob>.Instance);
        }

        private static PerformContext ContextOnRetry(int retryCount)
        {
            var connection = Substitute.For<IStorageConnection>();
            connection.GetJobParameter("job-1", "RetryCount").Returns(retryCount.ToString());
            var job = Job.FromExpression<IOrderProcessingJob>(j => j.ProcessAsync(OrderId, null, null, CancellationToken.None));

            return new PerformContext(
                null,
                connection,
                new BackgroundJob("job-1", job, DateTime.UtcNow),
                Substitute.For<IJobCancellationToken>());
        }

        [TestMethod]
        public async Task ProcessAsync_UsesIncomingCorrelationIdAndProcessesTheOrder()
        {
            var job = CreateJob();

            await job.ProcessAsync(OrderId, "corr-from-message", null, CancellationToken.None);

            Assert.AreEqual("corr-from-message", _correlation.CorrelationId);
            await _orders.Received(1).ProcessAsync(OrderId, Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task ProcessAsync_WhenAnEarlierAttemptFails_RethrowsWithoutMarkingFailed()
        {
            _orders.ProcessAsync(OrderId, Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("db down"));
            var job = CreateJob();

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
                job.ProcessAsync(OrderId, null, ContextOnRetry(JobRetryAttempts.OrderProcessing - 1), CancellationToken.None));

            await _failureOrders.DidNotReceiveWithAnyArgs().MarkFailedAsync(default, default);
        }

        [TestMethod]
        public async Task ProcessAsync_WhenTheLastRetryFails_MarksOrderFailedAndRethrows()
        {
            _orders.ProcessAsync(OrderId, Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("db down"));
            var job = CreateJob();

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
                job.ProcessAsync(OrderId, null, ContextOnRetry(JobRetryAttempts.OrderProcessing), CancellationToken.None));

            await _failureOrders.Received(1).MarkFailedAsync(OrderId, CancellationToken.None);
        }

        [TestMethod]
        public void ProcessAsync_IsRetriedAsManyTimesAsConfigured()
        {
            var attribute = typeof(IOrderProcessingJob)
                .GetMethod(nameof(IOrderProcessingJob.ProcessAsync))!
                .GetCustomAttributes(typeof(AutomaticRetryAttribute), inherit: false)
                .Cast<AutomaticRetryAttribute>()
                .Single();

            Assert.AreEqual(JobRetryAttempts.OrderProcessing, attribute.Attempts);
        }
    }
}
