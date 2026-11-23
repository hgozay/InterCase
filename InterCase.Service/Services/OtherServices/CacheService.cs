using System.Text;
using System.Text.Json;
using InterCase.Common;
using InterCase.Service.Helpers;
using InterCase.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace InterCase.Service.Services.OtherServices
{
    public sealed class CacheService(
        ICacheManager manager,
        ILogger<CacheService> logger) : ICacheService
    {
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var payload = await manager.GetAsync(key);
                return payload is null
                    ? default
                    : JsonSerializer.Deserialize<T>(payload, JsonDefaults.SerializerOptions);
            }
            catch (Exception ex) when (ex is CacheUnavailableException or JsonException)
            {
                logger.LogWarning(ex, "Cache read failed for {CacheKey}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
        {
            try
            {
                await manager.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(value, JsonDefaults.SerializerOptions), ttl);
            }
            catch (CacheUnavailableException ex)
            {
                logger.LogWarning(ex, "Cache write failed for {CacheKey}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await manager.RemoveAsync(key);
            }
            catch (CacheUnavailableException ex)
            {
                logger.LogWarning(ex, "Cache remove failed for {CacheKey}", key);
            }
        }

        public async Task<string?> GetStringAsync(string key)
        {
            try
            {
                var payload = await manager.GetAsync(key);
                return payload is null ? null : Encoding.UTF8.GetString(payload);
            }
            catch (CacheUnavailableException ex)
            {
                logger.LogWarning(ex, "Cache read failed for {CacheKey}", key);
                return null;
            }
        }

        public async Task SetStringAsync(string key, string value)
        {
            try
            {
                await manager.SetAsync(key, Encoding.UTF8.GetBytes(value));
            }
            catch (CacheUnavailableException ex)
            {
                logger.LogWarning(ex, "Cache write failed for {CacheKey}", key);
            }
        }
    }
}
