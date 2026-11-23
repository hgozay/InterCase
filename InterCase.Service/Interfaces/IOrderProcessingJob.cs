using Hangfire;
using Hangfire.Server;
using InterCase.Common;

namespace InterCase.Service.Interfaces
{
    public interface IOrderProcessingJob
    {
        [AutomaticRetry(Attempts = JobRetryAttempts.OrderProcessing)]
        Task ProcessAsync(long orderId, string? correlationId, PerformContext? context, CancellationToken ct);
    }
}
