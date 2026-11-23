namespace InterCase.Common.Dtos.Workers
{
    public sealed record WorkerStatusResponse(bool IsRunning, int ActiveServers, DateTime? LastHeartbeatAt);
}
