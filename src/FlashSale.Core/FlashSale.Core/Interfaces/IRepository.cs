using FlashSale.Core.Entities;

namespace FlashSale.Core.Interfaces;

/// <summary>
/// Interface base para repositórios.
/// </summary>
/// <typeparam name="T">Tipo da entidade.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Busca uma entidade pelo ID.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma nova entidade.
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma entidade existente.
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove uma entidade.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
