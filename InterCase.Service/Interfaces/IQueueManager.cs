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
    }
}
