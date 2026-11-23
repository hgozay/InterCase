namespace InterCase.Service.Interfaces
{
    public interface IQueueService
    {
        Task PublishAsync<TMessage>(
            string queueName,
            TMessage message,
            string? correlationId = null,
            CancellationToken ct = default);
    }
}
