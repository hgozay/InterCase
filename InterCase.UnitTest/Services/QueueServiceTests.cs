using System.Text;
using System.Text.Json;
using InterCase.Common;
using InterCase.Service.Helpers;
using InterCase.Service.Interfaces;
using InterCase.Service.Services.OtherServices;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace InterCase.UnitTest.Services
{
    [TestClass]
    public sealed class QueueServiceTests
    {
        private const string Queue = "order-placed";

        private readonly IQueueManager _manager = Substitute.For<IQueueManager>();

        private QueueService CreateService() => new(_manager, NullLogger<QueueService>.Instance);

        private static OrderPlacedMessage Message() => new(
            42,
            Guid.NewGuid(),
            Guid.NewGuid(),
            3,
            PaymentMethod.Cash,
            new DateTime(2026, 11, 23, 12, 0, 0, DateTimeKind.Utc),
            "corr-1");

        private Func<ReadOnlyMemory<byte>, string?, CancellationToken, Task<QueueHandleResult>> CaptureHandler()
        {
            Func<ReadOnlyMemory<byte>, string?, CancellationToken, Task<QueueHandleResult>>? captured = null;
            _manager.SubscribeAsync(
                    Queue,
                    Arg.Do<Func<ReadOnlyMemory<byte>, string?, CancellationToken, Task<QueueHandleResult>>>(h => captured = h),
                    Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);
            return (body, correlationId, ct) => captured!(body, correlationId, ct);
        }

        [TestMethod]
        public async Task PublishAsync_SendsCamelCaseJsonWithCorrelationIdAndMessageId()
        {
            byte[]? body = null;
            string? messageId = null;
            _manager.PublishAsync(
                    Queue,
                    Arg.Do<ReadOnlyMemory<byte>>(b => body = b.ToArray()),
                    Arg.Do<string>(id => messageId = id),
                    Arg.Any<string?>(),
                    Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);
            var message = Message();

            await CreateService().PublishAsync(Queue, message, "corr-1");

            await _manager.Received(1).PublishAsync(Queue, Arg.Any<ReadOnlyMemory<byte>>(), Arg.Any<string>(), "corr-1", Arg.Any<CancellationToken>());
            var json = Encoding.UTF8.GetString(body!);
            StringAssert.Contains(json, "\"orderId\":42");
            StringAssert.Contains(json, "\"paymentMethod\":\"Cash\"");
            Assert.AreEqual(message, JsonSerializer.Deserialize<OrderPlacedMessage>(body, JsonDefaults.SerializerOptions));
            Assert.AreEqual(32, messageId!.Length);
        }

        [TestMethod]
        public async Task PublishAsync_UsesANewMessageIdForEveryMessage()
        {
            var ids = new List<string>();
            _manager.PublishAsync(Queue, Arg.Any<ReadOnlyMemory<byte>>(), Arg.Do<string>(ids.Add), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);
            var service = CreateService();

            await service.PublishAsync(Queue, Message());
            await service.PublishAsync(Queue, Message());

            Assert.HasCount(2, ids);
            Assert.AreNotEqual(ids[0], ids[1]);
        }

        [TestMethod]
        public async Task SubscribeAsync_WhenMessageIsValid_CallsHandlerAndAcks()
        {
            var handler = CaptureHandler();
            var message = Message();
            OrderPlacedMessage? received = null;
            string? receivedCorrelation = null;

            await CreateService().SubscribeAsync<OrderPlacedMessage>(Queue, (m, correlationId, _) =>
            {
                received = m;
                receivedCorrelation = correlationId;
                return Task.CompletedTask;
            }, CancellationToken.None);
            var result = await handler(JsonSerializer.SerializeToUtf8Bytes(message, JsonDefaults.SerializerOptions), "corr-9", CancellationToken.None);

            Assert.AreEqual(QueueHandleResult.Ack, result);
            Assert.AreEqual(message, received);
            Assert.AreEqual("corr-9", receivedCorrelation);
        }

        [TestMethod]
        [DataRow("not json")]
        [DataRow("null")]
        [DataRow("{\"orderId\":\"abc\"}")]
        public async Task SubscribeAsync_WhenMessageCannotBeRead_RejectsWithoutCallingHandler(string payload)
        {
            var handler = CaptureHandler();
            var called = false;

            await CreateService().SubscribeAsync<OrderPlacedMessage>(Queue, (_, _, _) =>
            {
                called = true;
                return Task.CompletedTask;
            }, CancellationToken.None);
            var result = await handler(Encoding.UTF8.GetBytes(payload), null, CancellationToken.None);

            Assert.AreEqual(QueueHandleResult.Reject, result);
            Assert.IsFalse(called);
        }

        [TestMethod]
        public async Task SubscribeAsync_WhenHandlerThrows_LetsTheManagerDecideOnRetry()
        {
            var handler = CaptureHandler();

            await CreateService().SubscribeAsync<OrderPlacedMessage>(Queue, (_, _, _) =>
                throw new InvalidOperationException("storage down"), CancellationToken.None);

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
                handler(JsonSerializer.SerializeToUtf8Bytes(Message(), JsonDefaults.SerializerOptions), null, CancellationToken.None));
        }
    }
}
