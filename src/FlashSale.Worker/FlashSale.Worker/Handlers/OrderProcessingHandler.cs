using FlashSale.Core.Entities;
using FlashSale.Core.Exceptions;
using FlashSale.Core.Interfaces;
using FlashSale.Worker.Messages;

namespace FlashSale.Worker.Handlers;

/// <summary>
/// Handler responsável por processar pedidos.
/// Implementa a lógica de negócio: validar estoque, decrementar, confirmar pedido.
/// </summary>
public class OrderProcessingHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<OrderProcessingHandler> _logger;

    public OrderProcessingHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ILogger<OrderProcessingHandler> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Processa um pedido: valida estoque, decrementa e confirma.
    /// </summary>
    /// <param name="message">Mensagem do pedido.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public async Task HandleAsync(OrderMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processando pedido. OrderId: {OrderId}, Items: {ItemCount}",
            message.OrderId, message.Items.Count);

        // 1. Buscar o pedido
        var order = await _orderRepository.GetByIdAsync(message.OrderId, cancellationToken);
        if (order == null)
        {
            _logger.LogWarning("Pedido {OrderId} não encontrado", message.OrderId);
            return;
        }

        // 2. Verificar se já foi processado (idempotência)
        if (order.Status != Core.Enums.OrderStatus.Pending)
        {
            _logger.LogInformation(
                "Pedido {OrderId} já processado. Status: {Status}",
                order.Id, order.Status);
            return;
        }

        try
        {
            // 3. Processar cada item
            decimal totalAmount = 0;

            foreach (var item in message.Items)
            {
                // Buscar produto com lock pessimista
                var product = await _productRepository.GetByIdForUpdateAsync(item.ProductId, cancellationToken);
                
                if (product == null)
                {
                    throw new ProductNotFoundException(item.ProductId);
                }

                // Verificar estoque
                if (!product.HasSufficientStock(item.Quantity))
                {
                    throw new InsufficientStockException(item.ProductId, item.Quantity, product.Stock);
                }

                // Decrementar estoque com optimistic locking
                var success = await _productRepository.UpdateStockAsync(
                    product.Id,
                    product.Stock - item.Quantity,
                    product.Version,
                    cancellationToken);

                if (!success)
                {
                    // Conflito de versão - retry será feito pelo consumer
                    throw new InvalidOperationException($"Conflito de versão ao atualizar estoque do produto {product.Id}");
                }

                // Atualizar preço no item do pedido
                var orderItem = order.Items.First(i => i.ProductId == item.ProductId);
                orderItem.UnitPrice = product.Price;
                totalAmount += product.Price * item.Quantity;

                _logger.LogDebug(
                    "Estoque decrementado. ProductId: {ProductId}, Quantidade: {Quantity}, NovoEstoque: {NewStock}",
                    product.Id, item.Quantity, product.Stock - item.Quantity);
            }

            // 4. Confirmar pedido
            order.TotalAmount = totalAmount;
            order.Confirm();
            await _orderRepository.UpdateAsync(order, cancellationToken);

            _logger.LogInformation(
                "Pedido confirmado. OrderId: {OrderId}, Total: {Total}",
                order.Id, totalAmount);

            // TODO: Enviar notificação via SignalR
        }
        catch (InsufficientStockException ex)
        {
            _logger.LogWarning(ex, "Estoque insuficiente para pedido {OrderId}", order.Id);
            order.Fail(ex.Message);
            await _orderRepository.UpdateAsync(order, cancellationToken);
        }
        catch (ProductNotFoundException ex)
        {
            _logger.LogWarning(ex, "Produto não encontrado para pedido {OrderId}", order.Id);
            order.Fail(ex.Message);
            await _orderRepository.UpdateAsync(order, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pedido {OrderId}", order.Id);
            order.Fail("Erro interno ao processar pedido");
            await _orderRepository.UpdateAsync(order, cancellationToken);
            throw; // Re-throw para o consumer decidir sobre retry
        }
    }
}
