using Hangfire;
using InterCase.Common;
using InterCase.Service.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InterCase.Service.Consumers
{
    public sealed class OrderPlacedConsumer(
        IQueueService queue,
        IBackgroundJobClient jobs,
        ILogger<OrderPlacedConsumer> logger) : BackgroundService
    {
        private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await queue.SubscribeAsync<OrderPlacedMessage>(QueueNames.OrderPlaced, HandleAsync, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Consumer for {QueueName} stopped; reconnecting in {DelaySeconds} s", QueueNames.OrderPlaced, ReconnectDelay.TotalSeconds);
                }

                try
                {
                    await Task.Delay(ReconnectDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private Task HandleAsync(OrderPlacedMessage message, string? correlationId, CancellationToken ct)
        {
            var effectiveCorrelationId = correlationId ?? message.CorrelationId;

            using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = effectiveCorrelationId, ["OrderId"] = message.OrderId }))
            {
                var jobId = jobs.Enqueue<IOrderProcessingJob>(job =>
                    job.ProcessAsync(message.OrderId, effectiveCorrelationId, null, CancellationToken.None));

                logger.LogInformation("OrderPlaced received for order {OrderId}; job {JobId} enqueued", message.OrderId, jobId);
            }

            return Task.CompletedTask;
        }
    }
}
