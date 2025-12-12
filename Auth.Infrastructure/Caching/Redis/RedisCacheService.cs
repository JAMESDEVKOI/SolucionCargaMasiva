using Auth.Application.Abstractions.Interfaces.Caching;
using StackExchange.Redis;
using System.Text.Json;

namespace Auth.Infrastructure.Caching.Redis
{
    internal sealed class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = redis.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(
            string key,
            CancellationToken cancellationToken = default)
        {
            var value = await _database.StringGetAsync(key);

            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? expiry = null,
            CancellationToken cancellationToken = default)
        {
            var serializedValue = JsonSerializer.Serialize(value);

            await _database.StringSetAsync(
                key,
                serializedValue,
                expiry);
        }

        public async Task<bool> RemoveAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            return await _database.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            return await _database.KeyExistsAsync(key);
        }
    }
}
