using System.Text.Json;
using FlashSale.Worker.Handlers;
using FlashSale.Worker.Messages;
using StackExchange.Redis;

namespace FlashSale.Worker.Services;

/// <summary>
/// Serviço de consumo de pedidos do Redis Stream.
/// Usa Consumer Groups para garantir processamento único.
/// </summary>
public class OrderConsumerService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderConsumerService> _logger;

    private const string StreamName = "orders:pending";
    private const string GroupName = "order-processors";
    private readonly string _consumerName;

    public OrderConsumerService(
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory,
        ILogger<OrderConsumerService> logger)
    {
        _redis = redis;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _consumerName = $"worker-{Environment.MachineName}-{Guid.NewGuid():N}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "OrderConsumerService iniciando. Consumer: {Consumer}, Stream: {Stream}, Group: {Group}",
            _consumerName, StreamName, GroupName);

        var db = _redis.GetDatabase();

        // Garantir que o Consumer Group existe
        await EnsureConsumerGroupExistsAsync(db);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Ler mensagens do stream usando XREADGROUP
                var entries = await db.StreamReadGroupAsync(
                    StreamName,
                    GroupName,
                    _consumerName,
                    ">",  // Apenas novas mensagens
                    count: 10);

                if (entries == null || entries.Length == 0)
                {
                    // Sem mensagens, aguardar antes de tentar novamente
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                _logger.LogDebug("Recebidas {Count} mensagens do stream", entries.Length);

                foreach (var entry in entries)
                {
                    await ProcessMessageAsync(db, entry, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Shutdown graceful
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consumir mensagens do stream");
                await Task.Delay(5000, stoppingToken); // Backoff em caso de erro
            }
        }

        _logger.LogInformation("OrderConsumerService finalizando");
    }

    private async Task ProcessMessageAsync(IDatabase db, StreamEntry entry, CancellationToken stoppingToken)
    {
        var messageId = entry.Id.ToString();

        try
        {
            // Deserializar payload
            var payloadValue = entry["payload"];
            if (!payloadValue.HasValue)
            {
                _logger.LogWarning("Mensagem {MessageId} sem payload", messageId);
                await AcknowledgeAsync(db, messageId);
                return;
            }

            var message = JsonSerializer.Deserialize<OrderMessage>(payloadValue!);
            if (message == null)
            {
                _logger.LogWarning("Falha ao deserializar mensagem {MessageId}", messageId);
                await AcknowledgeAsync(db, messageId);
                return;
            }

            _logger.LogInformation(
                "Processando mensagem. MessageId: {MessageId}, OrderId: {OrderId}",
                messageId, message.OrderId);

            // Processar usando um novo scope (para DI scoped)
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<OrderProcessingHandler>();

            await handler.HandleAsync(message, stoppingToken);

            // ACK após sucesso
            await AcknowledgeAsync(db, messageId);

            _logger.LogInformation(
                "Mensagem processada com sucesso. MessageId: {MessageId}, OrderId: {OrderId}",
                messageId, message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem {MessageId}", messageId);
            // NÃO fazer ACK - mensagem ficará pendente para retry
            // TODO: Implementar Dead Letter Queue após N tentativas
        }
    }

    private async Task AcknowledgeAsync(IDatabase db, string messageId)
    {
        await db.StreamAcknowledgeAsync(StreamName, GroupName, messageId);
    }

    private async Task EnsureConsumerGroupExistsAsync(IDatabase db)
    {
        try
        {
            await db.StreamCreateConsumerGroupAsync(StreamName, GroupName, "0", createStream: true);
            _logger.LogInformation("Consumer Group {Group} criado no stream {Stream}", GroupName, StreamName);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Grupo já existe
            _logger.LogDebug("Consumer Group {Group} já existe", GroupName);
        }
    }
}
