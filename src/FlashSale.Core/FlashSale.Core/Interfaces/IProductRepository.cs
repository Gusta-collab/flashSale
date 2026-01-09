using FlashSale.Core.Entities;

namespace FlashSale.Core.Interfaces;

/// <summary>
/// Interface para repositório de produtos.
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Busca todos os produtos ativos.
    /// </summary>
    Task<IEnumerable<Product>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um produto com lock pessimista para atualização de estoque.
    /// </summary>
    Task<Product?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza o estoque de um produto com verificação de versão (optimistic locking).
    /// </summary>
    /// <returns>True se atualizado com sucesso, False se houve conflito de versão.</returns>
    Task<bool> UpdateStockAsync(Guid id, int newStock, int expectedVersion, CancellationToken cancellationToken = default);
}
