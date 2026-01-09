namespace FlashSale.Api.DTOs;

/// <summary>
/// Mensagem de pedido para o Redis Stream.
/// </summary>
public class OrderStreamMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public List<OrderItemStreamMessage> Items { get; set; } = new();
}

/// <summary>
/// Item do pedido para o stream.
/// </summary>
public class OrderItemStreamMessage
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
