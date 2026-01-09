using System.Text.Json;
using FlashSale.Core.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FlashSale.Infrastructure.Redis;

/// <summary>
/// Implementação de cache usando Redis.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);

        if (!value.HasValue)
        {
            _logger.LogDebug("Cache miss para chave {Key}", key);
            return null;
        }

        _logger.LogDebug("Cache hit para chave {Key}", key);
        return JsonSerializer.Deserialize<T>(value!);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(
        string key, 
        T value, 
        TimeSpan? expiration = null, 
        CancellationToken cancellationToken = default) where T : class
    {
        var db = _redis.GetDatabase();
        var serialized = JsonSerializer.Serialize(value);

        await db.StringSetAsync(key, serialized, expiration ?? DefaultExpiration);

        _logger.LogDebug("Cache set para chave {Key}, TTL: {TTL}", key, expiration ?? DefaultExpiration);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);

        _logger.LogDebug("Cache removido para chave {Key}", key);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync(key);
    }
}
