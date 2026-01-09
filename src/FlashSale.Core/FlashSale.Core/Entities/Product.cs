namespace FlashSale.Core.Entities;

/// <summary>
/// Representa um produto disponível para venda.
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Nome do produto.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada do produto.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Preço unitário em reais.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Quantidade disponível em estoque.
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// Versão para Optimistic Locking.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Indica se o produto está ativo para venda.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Verifica se há estoque suficiente para a quantidade solicitada.
    /// </summary>
    /// <param name="quantity">Quantidade desejada.</param>
    /// <returns>True se há estoque suficiente.</returns>
    public bool HasSufficientStock(int quantity) => Stock >= quantity;

    /// <summary>
    /// Decrementa o estoque do produto.
    /// </summary>
    /// <param name="quantity">Quantidade a decrementar.</param>
    /// <exception cref="InvalidOperationException">Se não houver estoque suficiente.</exception>
    public void DecrementStock(int quantity)
    {
        if (!HasSufficientStock(quantity))
            throw new InvalidOperationException($"Estoque insuficiente. Disponível: {Stock}, Solicitado: {quantity}");

        Stock -= quantity;
        Version++;
    }
}
