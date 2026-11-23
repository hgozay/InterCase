using System.Text;
using System.Text.Json;
using InterCase.Common;
using InterCase.Service.Helpers;
using InterCase.Service.Interfaces;
using InterCase.Service.Services.OtherServices;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace InterCase.UnitTest.Services
{
    [TestClass]
    public sealed class CacheServiceTests
    {
        private const string Key = "orders:user:test";

        private readonly ICacheManager _manager = Substitute.For<ICacheManager>();
        private readonly CacheUnavailableException _unavailable = new("redis down", new TimeoutException());

        private CacheService CreateService() => new(_manager, NullLogger<CacheService>.Instance);

        public sealed record Sample(string Name, PaymentMethod Method);

        [TestMethod]
        public async Task GetAsync_WhenKeyExists_ReturnsDeserializedValue()
        {
            var stored = new Sample("Ayşe", PaymentMethod.BankTransfer);
            _manager.GetAsync(Key).Returns(JsonSerializer.SerializeToUtf8Bytes(stored, JsonDefaults.SerializerOptions));

            var result = await CreateService().GetAsync<Sample>(Key);

            Assert.AreEqual(stored, result);
        }

        [TestMethod]
        public async Task GetAsync_WhenKeyIsMissing_ReturnsNull()
        {
            _manager.GetAsync(Key).Returns((byte[]?)null);

            var result = await CreateService().GetAsync<Sample>(Key);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAsync_WhenPayloadIsNotJson_ReturnsNull()
        {
            _manager.GetAsync(Key).Returns(Encoding.UTF8.GetBytes("not json"));

            var result = await CreateService().GetAsync<Sample>(Key);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAsync_WhenCacheIsUnavailable_ReturnsNull()
        {
            _manager.GetAsync(Key).ThrowsAsync(_unavailable);

            var result = await CreateService().GetAsync<Sample>(Key);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SetAsync_StoresJsonWithTheGivenTtl()
        {
            byte[]? written = null;
            _manager.SetAsync(Key, Arg.Do<byte[]>(bytes => written = bytes), Arg.Any<TimeSpan?>()).Returns(Task.CompletedTask);
            var value = new Sample("Mehmet", PaymentMethod.Cash);

            await CreateService().SetAsync(Key, value, TimeSpan.FromSeconds(120));

            await _manager.Received(1).SetAsync(Key, Arg.Any<byte[]>(), TimeSpan.FromSeconds(120));
            Assert.AreEqual(value, JsonSerializer.Deserialize<Sample>(written!, JsonDefaults.SerializerOptions));
            StringAssert.Contains(Encoding.UTF8.GetString(written!), "\"method\":\"Cash\"");
        }

        [TestMethod]
        public async Task SetAsync_WhenCacheIsUnavailable_DoesNotThrow()
        {
            _manager.SetAsync(Key, Arg.Any<byte[]>(), Arg.Any<TimeSpan?>()).ThrowsAsync(_unavailable);

            await CreateService().SetAsync(Key, new Sample("x", PaymentMethod.Cash), TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public async Task RemoveAsync_WhenCacheIsUnavailable_DoesNotThrow()
        {
            _manager.RemoveAsync(Key).ThrowsAsync(_unavailable);

            await CreateService().RemoveAsync(Key);

            await _manager.Received(1).RemoveAsync(Key);
        }

        [TestMethod]
        public async Task SetStringAsync_StoresUtf8TextWithoutExpiry()
        {
            const string text = "Order 5 işlendi: ğüşıöç";

            await CreateService().SetStringAsync(Key, text);

            await _manager.Received(1).SetAsync(Key, Arg.Is<byte[]>(bytes => Encoding.UTF8.GetString(bytes) == text), null);
        }

        [TestMethod]
        public async Task GetStringAsync_ReturnsStoredTextOrNull()
        {
            const string text = "Order 5 processed at 2026-11-23T12:00:00Z";
            _manager.GetAsync("present").Returns(Encoding.UTF8.GetBytes(text));
            _manager.GetAsync("absent").Returns((byte[]?)null);
            var service = CreateService();

            Assert.AreEqual(text, await service.GetStringAsync("present"));
            Assert.IsNull(await service.GetStringAsync("absent"));
        }

        [TestMethod]
        public async Task GetStringAsync_WhenCacheIsUnavailable_ReturnsNull()
        {
            _manager.GetAsync(Key).ThrowsAsync(_unavailable);

            var result = await CreateService().GetStringAsync(Key);

            Assert.IsNull(result);
        }
    }
}
