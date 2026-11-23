using System.Text.Json;
using InterCase.Common;
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

        public Task SubscribeAsync<TMessage>(
            string queueName,
            Func<TMessage, string?, CancellationToken, Task> handler,
            CancellationToken ct) =>
            manager.SubscribeAsync(queueName, async (body, correlationId, token) =>
            {
                TMessage? message;
                try
                {
                    message = JsonSerializer.Deserialize<TMessage>(body.Span, JsonDefaults.SerializerOptions);
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "Message on {QueueName} could not be read and was rejected", queueName);
                    return QueueHandleResult.Reject;
                }

                if (message is null)
                {
                    logger.LogError("Empty message on {QueueName} was rejected", queueName);
                    return QueueHandleResult.Reject;
                }

                await handler(message, correlationId, token);
                return QueueHandleResult.Ack;
            }, ct);
    }
}
