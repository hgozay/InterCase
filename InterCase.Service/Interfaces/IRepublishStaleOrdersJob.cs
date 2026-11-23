using Hangfire;

namespace InterCase.Service.Interfaces
{
    public interface IRepublishStaleOrdersJob
    {
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(timeoutInSeconds: 60)]
        Task RunAsync(CancellationToken ct);
    }
}
