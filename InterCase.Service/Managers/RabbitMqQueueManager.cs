using System.Reflection;
using InterCase.Common;
using InterCase.Service.Helpers;
using InterCase.Service.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace InterCase.Service.Managers
{
    public sealed class RabbitMqQueueManager(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqQueueManager> logger) : IQueueManager, IAsyncDisposable
    {
        private static readonly CreateChannelOptions ChannelOptions = new(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

        private readonly SemaphoreSlim _lock = new(1, 1);
        private IConnection? _connection;

        public async Task PublishAsync(
            string queueName,
            ReadOnlyMemory<byte> body,
            string messageId,
            string? correlationId,
            CancellationToken ct = default)
        {
            var connection = await GetConnectionAsync(ct);
            await using var channel = await connection.CreateChannelAsync(ChannelOptions, ct);
            await RabbitMqTopology.DeclareQueueAsync(channel, queueName, ct);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                MessageId = messageId,
                CorrelationId = correlationId
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: ct);
        }

        public async Task SubscribeAsync(
            string queueName,
            Func<ReadOnlyMemory<byte>, string?, CancellationToken, Task<QueueHandleResult>> handler,
            CancellationToken ct)
        {
            var connection = await GetConnectionAsync(ct);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: options.Value.PrefetchCount, global: false, cancellationToken: ct);
            await RabbitMqTopology.DeclareQueueAsync(channel, queueName, ct);

            var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            channel.ChannelShutdownAsync += (_, _) =>
            {
                closed.TrySetResult();
                return Task.CompletedTask;
            };

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, delivery) =>
            {
                var result = await HandleAsync(queueName, delivery, handler, ct);
                await CompleteAsync(channel, queueName, delivery, result, ct);
            };

            await channel.BasicConsumeAsync(queueName, autoAck: false, consumer, ct);
            logger.LogInformation("Consuming {QueueName} with prefetch {PrefetchCount}", queueName, options.Value.PrefetchCount);

            using (ct.Register(() => closed.TrySetResult()))
            {
                await closed.Task;
            }

            if (!ct.IsCancellationRequested)
                throw new InvalidOperationException($"The channel consuming {queueName} was closed.");
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is null)
                return;

            if (_connection.IsOpen)
                await _connection.CloseAsync();

            await _connection.DisposeAsync();
            _connection = null;
            _lock.Dispose();
        }

        private async Task<QueueHandleResult> HandleAsync(
            string queueName,
            BasicDeliverEventArgs delivery,
            Func<ReadOnlyMemory<byte>, string?, CancellationToken, Task<QueueHandleResult>> handler,
            CancellationToken ct)
        {
            try
            {
                return await handler(delivery.Body, delivery.BasicProperties.CorrelationId, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Handling message {MessageId} from {QueueName} failed; it will be requeued", delivery.BasicProperties.MessageId, queueName);
                return QueueHandleResult.Retry;
            }
        }

        private async Task CompleteAsync(
            IChannel channel,
            string queueName,
            BasicDeliverEventArgs delivery,
            QueueHandleResult result,
            CancellationToken ct)
        {
            try
            {
                switch (result)
                {
                    case QueueHandleResult.Ack:
                        await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false);
                        break;
                    case QueueHandleResult.Reject:
                        await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false);
                        break;
                    default:
                        if (!ct.IsCancellationRequested)
                            await Task.Delay(RetryDelay, CancellationToken.None);
                        await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: true);
                        break;
                }
            }
            catch (Exception ex) when (!channel.IsOpen)
            {
                logger.LogWarning(ex, "Channel for {QueueName} closed before message {MessageId} was settled; the broker will redeliver it", queueName, delivery.BasicProperties.MessageId);
            }
        }

        private async Task<IConnection> GetConnectionAsync(CancellationToken ct)
        {
            if (_connection is { IsOpen: true })
                return _connection;

            await _lock.WaitAsync(ct);
            try
            {
                if (_connection is { IsOpen: true })
                    return _connection;

                if (_connection is not null)
                    await _connection.DisposeAsync();

                _connection = await CreateFactory().CreateConnectionAsync(ct);
                logger.LogInformation("RabbitMQ connection opened to {HostName}:{Port}", options.Value.HostName, options.Value.Port);
                return _connection;
            }
            finally
            {
                _lock.Release();
            }
        }

        private ConnectionFactory CreateFactory()
        {
            var settings = options.Value;

            return new ConnectionFactory
            {
                HostName = settings.HostName,
                Port = settings.Port,
                UserName = settings.UserName,
                Password = settings.Password,
                ClientProvidedName = Assembly.GetEntryAssembly()?.GetName().Name,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(5)
            };
        }
    }
}
