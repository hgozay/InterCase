using System.Reflection;
using InterCase.Common;
using InterCase.Service.Helpers;
using InterCase.Service.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace InterCase.Service.Managers
{
    public sealed class RabbitMqQueueManager(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqQueueManager> logger) : IQueueManager, IAsyncDisposable
    {
        private static readonly CreateChannelOptions ChannelOptions = new(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

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
