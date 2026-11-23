using InterCase.Common;

namespace InterCase.Service.Interfaces
{
    public interface IQueueManager
    {
        Task PublishAsync(
            string queueName,
            ReadOnlyMemory<byte> body,
            string messageId,
            string? correlationId,
            CancellationToken ct = default);

        Task SubscribeAsync(
            string queueName,
            Func<ReadOnlyMemory<byte>, string?, CancellationToken, Task<QueueHandleResult>> handler,
            CancellationToken ct);
    }
}
