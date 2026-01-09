namespace FlashSale.Core.Enums;

/// <summary>
/// Tipo de movimentação de estoque.
/// </summary>
public enum StockMovementType
{
    /// <summary>Entrada de estoque (compra, devolução).</summary>
    In = 0,
    
    /// <summary>Saída de estoque (venda).</summary>
    Out = 1,
    
    /// <summary>Reserva temporária (pedido pendente).</summary>
    Reserved = 2,
    
    /// <summary>Liberação de reserva (pedido cancelado).</summary>
    Released = 3
}
