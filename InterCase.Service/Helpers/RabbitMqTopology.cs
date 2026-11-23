using RabbitMQ.Client;

namespace InterCase.Service.Helpers
{
    public static class RabbitMqTopology
    {
        public static Task<QueueDeclareOk> DeclareQueueAsync(IChannel channel, string queueName, CancellationToken ct = default) =>
            channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: ct);
    }
}
