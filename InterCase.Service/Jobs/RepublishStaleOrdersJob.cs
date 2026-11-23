using InterCase.Common;
using InterCase.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace InterCase.Service.Jobs
{
    public sealed class RepublishStaleOrdersJob(
        IServiceManager serviceManager,
        ICorrelationContext correlation,
        ILogger<RepublishStaleOrdersJob> logger) : IRepublishStaleOrdersJob
    {
        public async Task RunAsync(CancellationToken ct)
        {
            using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlation.CorrelationId }))
            {
                await serviceManager.Order.RepublishStalePendingAsync(ct);
            }
        }
    }
}
