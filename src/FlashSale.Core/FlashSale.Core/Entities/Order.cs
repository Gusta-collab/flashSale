using FlashSale.Core.Enums;

namespace FlashSale.Core.Entities;

/// <summary>
/// Representa um pedido de compra.
/// </summary>
public class Order : BaseEntity
{
    /// <summary>
    /// ID do cliente que fez o pedido.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Cliente que fez o pedido.
    /// </summary>
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// Status atual do pedido.
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// Valor total do pedido.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Chave de idempotência para evitar pedidos duplicados.
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem de erro caso o pedido falhe.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Data de processamento do pedido.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Itens do pedido.
    /// </summary>
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    /// <summary>
    /// Parâmetros UTM para tracking.
    /// </summary>
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }

    /// <summary>
    /// Calcula o valor total baseado nos itens.
    /// </summary>
    public void CalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.UnitPrice * i.Quantity);
    }

    /// <summary>
    /// Marca o pedido como confirmado.
    /// </summary>
    public void Confirm()
    {
        Status = OrderStatus.Confirmed;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca o pedido como falho.
    /// </summary>
    /// <param name="reason">Motivo da falha.</param>
    public void Fail(string reason)
    {
        Status = OrderStatus.Failed;
        ErrorMessage = reason;
        ProcessedAt = DateTime.UtcNow;
    }
}
