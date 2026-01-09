namespace FlashSale.Core.Entities;

/// <summary>
/// Representa um item de um pedido.
/// </summary>
public class OrderItem : BaseEntity
{
    /// <summary>
    /// ID do pedido.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Pedido ao qual o item pertence.
    /// </summary>
    public virtual Order? Order { get; set; }

    /// <summary>
    /// ID do produto.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Produto do item.
    /// </summary>
    public virtual Product? Product { get; set; }

    /// <summary>
    /// Quantidade solicitada.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Preço unitário no momento da compra.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Subtotal do item (Quantity * UnitPrice).
    /// </summary>
    public decimal Subtotal => Quantity * UnitPrice;
}
