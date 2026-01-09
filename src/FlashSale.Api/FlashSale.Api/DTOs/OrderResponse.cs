using FlashSale.Core.Enums;

namespace FlashSale.Api.DTOs;

/// <summary>
/// Response com dados do pedido.
/// </summary>
public class OrderResponse
{
    /// <summary>
    /// ID do pedido.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Status atual.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Valor total.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Mensagem de erro (se houver).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Data de criação.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data de processamento.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Itens do pedido.
    /// </summary>
    public List<OrderItemResponse> Items { get; set; } = new();
}

/// <summary>
/// Response de item do pedido.
/// </summary>
public class OrderItemResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

/// <summary>
/// Response simplificado para criação de pedido (202 Accepted).
/// </summary>
public class OrderAcceptedResponse
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = "Pending";
    public string Message { get; set; } = "Pedido recebido e será processado em instantes.";
}
