namespace FlashSale.Api.DTOs;

/// <summary>
/// Request para criar um novo pedido.
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// ID do cliente.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Chave de idempotência para evitar pedidos duplicados.
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>
    /// Itens do pedido.
    /// </summary>
    public List<OrderItemRequest> Items { get; set; } = new();

    /// <summary>
    /// Parâmetros UTM (opcional).
    /// </summary>
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
}

/// <summary>
/// Item de um pedido.
/// </summary>
public class OrderItemRequest
{
    /// <summary>
    /// ID do produto.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Quantidade desejada.
    /// </summary>
    public int Quantity { get; set; }
}
