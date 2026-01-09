using FlashSale.Core.Entities;
using FlashSale.Core.Interfaces;
using FlashSale.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlashSale.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de produtos.
/// </summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Usa SELECT FOR UPDATE para lock pessimista no PostgreSQL.
    /// Garante que apenas uma transação pode modificar o produto por vez.
    /// </remarks>
    public async Task<Product?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // PostgreSQL: FOR UPDATE lock
        return await _dbSet
            .FromSqlRaw("SELECT * FROM products WHERE id = {0} FOR UPDATE", id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Implementa Optimistic Locking usando a coluna Version.
    /// Retorna false se a versão mudou (conflito detectado).
    /// </remarks>
    public async Task<bool> UpdateStockAsync(
        Guid id, 
        int newStock, 
        int expectedVersion, 
        CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
            @"UPDATE products 
              SET stock = {0}, 
                  version = version + 1,
                  updated_at = {1}
              WHERE id = {2} AND version = {3}",
            newStock,
            DateTime.UtcNow,
            id,
            expectedVersion,
            cancellationToken);

        return rowsAffected > 0;
    }
}
