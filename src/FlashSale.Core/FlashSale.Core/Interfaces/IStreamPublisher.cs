namespace FlashSale.Core.Interfaces;

/// <summary>
/// Interface para publicação de mensagens no Redis Streams.
/// </summary>
public interface IStreamPublisher
{
    /// <summary>
    /// Publica uma mensagem no stream especificado.
    /// </summary>
    /// <typeparam name="T">Tipo da mensagem.</typeparam>
    /// <param name="streamName">Nome do stream.</param>
    /// <param name="message">Mensagem a ser publicada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>ID da mensagem no stream.</returns>
    Task<string> PublishAsync<T>(string streamName, T message, CancellationToken cancellationToken = default) where T : class;
}

/// <summary>
/// Interface para consumo de mensagens do Redis Streams.
/// </summary>
public interface IStreamConsumer
{
    /// <summary>
    /// Lê mensagens do stream usando Consumer Group.
    /// </summary>
    /// <typeparam name="T">Tipo da mensagem.</typeparam>
    /// <param name="streamName">Nome do stream.</param>
    /// <param name="groupName">Nome do Consumer Group.</param>
    /// <param name="consumerName">Nome do consumidor.</param>
    /// <param name="count">Quantidade de mensagens a ler.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de mensagens.</returns>
    Task<IEnumerable<StreamMessage<T>>> ReadAsync<T>(
        string streamName, 
        string groupName, 
        string consumerName, 
        int count = 10,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Confirma processamento de uma mensagem (ACK).
    /// </summary>
    Task AcknowledgeAsync(string streamName, string groupName, string messageId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Mensagem do stream com metadados.
/// </summary>
public class StreamMessage<T> where T : class
{
    public string MessageId { get; set; } = string.Empty;
    public T Payload { get; set; } = default!;
    public DateTime Timestamp { get; set; }
}
