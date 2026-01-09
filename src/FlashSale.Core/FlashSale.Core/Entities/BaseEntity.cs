namespace FlashSale.Core.Entities;

/// <summary>
/// Entidade base para todas as entidades do domínio.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificador único da entidade.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Data de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data da última atualização.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
