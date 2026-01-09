using System.Text.Json;
using FlashSale.Core.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FlashSale.Infrastructure.Redis;

/// <summary>
/// Implementação do publisher de Redis Streams.
/// </summary>
public class RedisStreamPublisher : IStreamPublisher
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisStreamPublisher> _logger;

    public RedisStreamPublisher(
        IConnectionMultiplexer redis,
        ILogger<RedisStreamPublisher> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> PublishAsync<T>(
        string streamName, 
        T message, 
        CancellationToken cancellationToken = default) where T : class
    {
        var db = _redis.GetDatabase();

        // Serializar mensagem
        var payload = JsonSerializer.Serialize(message);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        // Adicionar ao stream
        var messageId = await db.StreamAddAsync(streamName, new NameValueEntry[]
        {
            new("payload", payload),
            new("timestamp", timestamp),
            new("type", typeof(T).Name)
        });

        _logger.LogInformation(
            "Mensagem publicada no stream. Stream: {Stream}, MessageId: {MessageId}, Type: {Type}",
            streamName, messageId, typeof(T).Name);

        return messageId!;
    }
}
