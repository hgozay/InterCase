using InterCase.Common;
using InterCase.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace InterCase.Service.Managers
{
    public sealed class RedisCacheManager(IConfiguration configuration) : ICacheManager, IDisposable
    {
        private readonly Lazy<ConnectionMultiplexer> _connection = new(() => Connect(configuration));

        public Task<byte[]?> GetAsync(string key) =>
            ExecuteAsync(async database => (byte[]?)await database.StringGetAsync(key));

        public Task SetAsync(string key, byte[] value, TimeSpan? ttl = null) =>
            ExecuteAsync(database => ttl is { } expiry
                ? database.StringSetAsync(key, value, expiry)
                : database.StringSetAsync(key, value));

        public Task RemoveAsync(string key) =>
            ExecuteAsync(database => database.KeyDeleteAsync(key));

        public void Dispose()
        {
            if (_connection.IsValueCreated)
                _connection.Value.Dispose();
        }

        private async Task<T> ExecuteAsync<T>(Func<IDatabase, Task<T>> operation)
        {
            try
            {
                return await operation(_connection.Value.GetDatabase());
            }
            catch (Exception ex) when (ex is RedisException or RedisTimeoutException)
            {
                throw new CacheUnavailableException("Redis is unavailable.", ex);
            }
        }

        private static ConnectionMultiplexer Connect(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("ConnectionStrings:Redis is not configured.");

            var options = ConfigurationOptions.Parse(connectionString);
            options.BacklogPolicy = BacklogPolicy.FailFast;

            return ConnectionMultiplexer.Connect(options);
        }
    }
}
