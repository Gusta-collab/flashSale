using FlashSale.Core.Entities;
using FlashSale.Core.Interfaces;
using FlashSale.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlashSale.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de pedidos.
/// </summary>
public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public override async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Busca por IdempotencyKey para verificar pedidos duplicados.
    /// </remarks>
    public async Task<Order?> GetByIdempotencyKeyAsync(
        string idempotencyKey, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(
        Guid customerId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);
    }
}
