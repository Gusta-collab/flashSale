namespace FlashSale.Worker.Messages;

/// <summary>
/// Mensagem de pedido recebida do Redis Stream.
/// </summary>
public class OrderMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public List<OrderItemMessage> Items { get; set; } = new();
}

/// <summary>
/// Item do pedido.
/// </summary>
public class OrderItemMessage
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
