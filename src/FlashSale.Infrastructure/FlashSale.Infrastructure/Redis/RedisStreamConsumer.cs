using System.Text.Json;
using FlashSale.Core.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FlashSale.Infrastructure.Redis;

/// <summary>
/// Implementação do consumer de Redis Streams.
/// </summary>
public class RedisStreamConsumer : IStreamConsumer
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisStreamConsumer> _logger;

    public RedisStreamConsumer(
        IConnectionMultiplexer redis,
        ILogger<RedisStreamConsumer> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StreamMessage<T>>> ReadAsync<T>(
        string streamName,
        string groupName,
        string consumerName,
        int count = 10,
        CancellationToken cancellationToken = default) where T : class
    {
        var db = _redis.GetDatabase();
        var messages = new List<StreamMessage<T>>();

        try
        {
            // Garantir que o grupo existe
            await EnsureConsumerGroupExistsAsync(db, streamName, groupName);

            // Ler mensagens pendentes usando ">" (novas mensagens)
            var entries = await db.StreamReadGroupAsync(
                streamName,
                groupName,
                consumerName,
                ">",
                count);

            if (entries == null || entries.Length == 0)
            {
                return messages;
            }

            foreach (var entry in entries)
            {
                try
                {
                    var payloadValue = entry["payload"];
                    var timestampValue = entry["timestamp"];

                    if (payloadValue.HasValue)
                    {
                        var payload = JsonSerializer.Deserialize<T>(payloadValue!);
                        if (payload != null)
                        {
                            messages.Add(new StreamMessage<T>
                            {
                                MessageId = entry.Id!,
                                Payload = payload,
                                Timestamp = long.TryParse(timestampValue, out var ts)
                                    ? DateTimeOffset.FromUnixTimeMilliseconds(ts).UtcDateTime
                                    : DateTime.UtcNow
                            });
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Erro ao deserializar mensagem {MessageId}", entry.Id);
                }
            }
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Erro ao ler mensagens do stream {Stream}", streamName);
        }

        return messages;
    }

    /// <inheritdoc />
    public async Task AcknowledgeAsync(
        string streamName, 
        string groupName, 
        string messageId, 
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.StreamAcknowledgeAsync(streamName, groupName, messageId);

        _logger.LogDebug(
            "Mensagem confirmada. Stream: {Stream}, Group: {Group}, MessageId: {MessageId}",
            streamName, groupName, messageId);
    }

    /// <summary>
    /// Cria o Consumer Group se não existir.
    /// </summary>
    private async Task EnsureConsumerGroupExistsAsync(IDatabase db, string streamName, string groupName)
    {
        try
        {
            // Tentar criar o grupo (ignora se já existe)
            await db.StreamCreateConsumerGroupAsync(streamName, groupName, "0", createStream: true);
            _logger.LogInformation("Consumer Group {Group} criado no stream {Stream}", groupName, streamName);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Grupo já existe, ignorar
        }
    }
}
