using FlashSale.Core.Entities;

namespace FlashSale.Core.Interfaces;

/// <summary>
/// Interface para repositório de pedidos.
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Busca um pedido pela chave de idempotência.
    /// </summary>
    Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca pedidos de um cliente específico.
    /// </summary>
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}
