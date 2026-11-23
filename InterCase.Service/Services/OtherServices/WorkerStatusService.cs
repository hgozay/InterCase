using Hangfire;
using InterCase.Common;
using InterCase.Common.Dtos.Workers;
using InterCase.Service.Interfaces;

namespace InterCase.Service.Services.OtherServices
{
    public sealed class WorkerStatusService(JobStorage storage, TimeProvider timeProvider) : IWorkerStatusService
    {
        public WorkerStatusResponse GetStatus()
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;
            var heartbeats = storage.GetMonitoringApi().Servers()
                .Where(server => server.Heartbeat.HasValue)
                .Select(server => DateTime.SpecifyKind(server.Heartbeat!.Value, DateTimeKind.Utc))
                .ToList();

            var activeServers = heartbeats.Count(heartbeat => now - heartbeat <= WorkerHeartbeat.Timeout);
            DateTime? lastHeartbeat = heartbeats.Count == 0 ? null : heartbeats.Max();

            return new WorkerStatusResponse(activeServers > 0, activeServers, lastHeartbeat);
        }
    }
}
