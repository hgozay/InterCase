using System.Diagnostics;
using Hangfire.Server;
using InterCase.Common;
using InterCase.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InterCase.Service.Jobs
{
    public sealed class OrderProcessingJob(
        IServiceManager serviceManager,
        IServiceScopeFactory scopeFactory,
        ICorrelationContext correlation,
        ILogger<OrderProcessingJob> logger) : IOrderProcessingJob
    {
        public async Task ProcessAsync(long orderId, string? correlationId, PerformContext? context, CancellationToken ct)
        {
            if (!string.IsNullOrEmpty(correlationId))
                correlation.CorrelationId = correlationId;

            using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlation.CorrelationId, ["OrderId"] = orderId }))
            {
                var stopwatch = Stopwatch.StartNew();
                var retryCount = context?.GetJobParameter<int>("RetryCount") ?? 0;
                logger.LogInformation("Processing order {OrderId} started (retry {RetryCount} of {MaxRetries})", orderId, retryCount, JobRetryAttempts.OrderProcessing);

                try
                {
                    await serviceManager.Order.ProcessAsync(orderId, ct);
                }
                catch (Exception ex) when (retryCount >= JobRetryAttempts.OrderProcessing && !ct.IsCancellationRequested)
                {
                    logger.LogError(ex, "Processing order {OrderId} failed on the last retry; marking it Failed", orderId);

                    await using var scope = scopeFactory.CreateAsyncScope();
                    await scope.ServiceProvider.GetRequiredService<IServiceManager>().Order.MarkFailedAsync(orderId, CancellationToken.None);
                    throw;
                }

                logger.LogInformation("Processing order {OrderId} finished in {ElapsedMs} ms", orderId, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
