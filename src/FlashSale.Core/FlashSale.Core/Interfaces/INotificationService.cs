namespace FlashSale.Core.Interfaces;

/// <summary>
/// Interface para serviço de notificações em tempo real.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Notifica que um pedido foi confirmado.
    /// </summary>
    Task NotifyOrderConfirmedAsync(Guid orderId, decimal totalAmount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifica que um pedido falhou.
    /// </summary>
    Task NotifyOrderFailedAsync(Guid orderId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifica atualização de status do pedido.
    /// </summary>
    Task NotifyOrderStatusChangedAsync(Guid orderId, string status, CancellationToken cancellationToken = default);
}
