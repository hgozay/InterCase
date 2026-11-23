using System.Text.Json;
using InterCase.Service.Helpers;
using InterCase.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace InterCase.Service.Services.OtherServices
{
    public sealed class QueueService(
        IQueueManager manager,
        ILogger<QueueService> logger) : IQueueService
    {
        public async Task PublishAsync<TMessage>(
            string queueName,
            TMessage message,
            string? correlationId = null,
            CancellationToken ct = default)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonDefaults.SerializerOptions);
            var messageId = Guid.NewGuid().ToString("N");

            await manager.PublishAsync(queueName, body, messageId, correlationId, ct);
            logger.LogInformation("Message {MessageId} published to {QueueName}", messageId, queueName);
        }
    }
}
