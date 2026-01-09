namespace FlashSale.Core.Enums;

/// <summary>
/// Status do pedido no sistema.
/// </summary>
public enum OrderStatus
{
    /// <summary>Pedido recebido, aguardando processamento.</summary>
    Pending = 0,
    
    /// <summary>Pedido em processamento pelo worker.</summary>
    Processing = 1,
    
    /// <summary>Pedido confirmado com sucesso.</summary>
    Confirmed = 2,
    
    /// <summary>Pedido falhou (estoque insuficiente, erro, etc).</summary>
    Failed = 3,
    
    /// <summary>Pedido cancelado pelo cliente ou sistema.</summary>
    Cancelled = 4
}
