namespace FlashSale.Core.Entities;

/// <summary>
/// Representa um cliente do sistema.
/// </summary>
public class Customer : BaseEntity
{
    /// <summary>
    /// Email do cliente (único).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nome completo do cliente.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Telefone do cliente.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Indica se o cliente está ativo.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Pedidos do cliente.
    /// </summary>
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
