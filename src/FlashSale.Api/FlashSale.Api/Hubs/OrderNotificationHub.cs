using Microsoft.AspNetCore.SignalR;

namespace FlashSale.Api.Hubs;

/// <summary>
/// Hub SignalR para notificações de pedidos em tempo real.
/// </summary>
public class OrderNotificationHub : Hub
{
    private readonly ILogger<OrderNotificationHub> _logger;

    public OrderNotificationHub(ILogger<OrderNotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Cliente se inscreve para receber atualizações de um pedido.
    /// </summary>
    /// <param name="orderId">ID do pedido.</param>
    public async Task SubscribeToOrder(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
        _logger.LogInformation(
            "Cliente {ConnectionId} inscrito no pedido {OrderId}",
            Context.ConnectionId, orderId);
    }

    /// <summary>
    /// Cliente cancela inscrição em um pedido.
    /// </summary>
    /// <param name="orderId">ID do pedido.</param>
    public async Task UnsubscribeFromOrder(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
        _logger.LogDebug(
            "Cliente {ConnectionId} removido do pedido {OrderId}",
            Context.ConnectionId, orderId);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Cliente conectado: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Cliente desconectado: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
