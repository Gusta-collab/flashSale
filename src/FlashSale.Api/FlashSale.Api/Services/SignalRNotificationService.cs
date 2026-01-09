using FlashSale.Api.Hubs;
using FlashSale.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace FlashSale.Api.Services;

/// <summary>
/// Implementação do serviço de notificações via SignalR.
/// </summary>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<OrderNotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<OrderNotificationHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task NotifyOrderConfirmedAsync(
        Guid orderId, 
        decimal totalAmount, 
        CancellationToken cancellationToken = default)
    {
        var groupName = $"order-{orderId}";

        await _hubContext.Clients.Group(groupName).SendAsync(
            "OrderConfirmed",
            new
            {
                orderId,
                status = "Confirmed",
                totalAmount,
                timestamp = DateTime.UtcNow
            },
            cancellationToken);

        _logger.LogInformation(
            "Notificação OrderConfirmed enviada. OrderId: {OrderId}",
            orderId);
    }

    /// <inheritdoc />
    public async Task NotifyOrderFailedAsync(
        Guid orderId, 
        string reason, 
        CancellationToken cancellationToken = default)
    {
        var groupName = $"order-{orderId}";

        await _hubContext.Clients.Group(groupName).SendAsync(
            "OrderFailed",
            new
            {
                orderId,
                status = "Failed",
                reason,
                timestamp = DateTime.UtcNow
            },
            cancellationToken);

        _logger.LogInformation(
            "Notificação OrderFailed enviada. OrderId: {OrderId}, Reason: {Reason}",
            orderId, reason);
    }

    /// <inheritdoc />
    public async Task NotifyOrderStatusChangedAsync(
        Guid orderId, 
        string status, 
        CancellationToken cancellationToken = default)
    {
        var groupName = $"order-{orderId}";

        await _hubContext.Clients.Group(groupName).SendAsync(
            "OrderStatusChanged",
            new
            {
                orderId,
                status,
                timestamp = DateTime.UtcNow
            },
            cancellationToken);

        _logger.LogDebug(
            "Notificação OrderStatusChanged enviada. OrderId: {OrderId}, Status: {Status}",
            orderId, status);
    }
}
