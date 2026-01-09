namespace FlashSale.Core.Exceptions;

/// <summary>
/// Exceção base para erros de domínio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exceção lançada quando não há estoque suficiente.
/// </summary>
public class InsufficientStockException : DomainException
{
    public Guid ProductId { get; }
    public int RequestedQuantity { get; }
    public int AvailableStock { get; }

    public InsufficientStockException(Guid productId, int requested, int available)
        : base($"Estoque insuficiente para produto {productId}. Solicitado: {requested}, Disponível: {available}")
    {
        ProductId = productId;
        RequestedQuantity = requested;
        AvailableStock = available;
    }
}

/// <summary>
/// Exceção lançada quando um pedido não é encontrado.
/// </summary>
public class OrderNotFoundException : DomainException
{
    public Guid OrderId { get; }

    public OrderNotFoundException(Guid orderId)
        : base($"Pedido {orderId} não encontrado")
    {
        OrderId = orderId;
    }
}

/// <summary>
/// Exceção lançada quando um pedido duplicado é detectado.
/// </summary>
public class DuplicateOrderException : DomainException
{
    public string IdempotencyKey { get; }
    public Guid ExistingOrderId { get; }

    public DuplicateOrderException(string idempotencyKey, Guid existingOrderId)
        : base($"Pedido duplicado detectado. IdempotencyKey: {idempotencyKey}")
    {
        IdempotencyKey = idempotencyKey;
        ExistingOrderId = existingOrderId;
    }
}

/// <summary>
/// Exceção lançada quando um produto não é encontrado.
/// </summary>
public class ProductNotFoundException : DomainException
{
    public Guid ProductId { get; }

    public ProductNotFoundException(Guid productId)
        : base($"Produto {productId} não encontrado")
    {
        ProductId = productId;
    }
}
