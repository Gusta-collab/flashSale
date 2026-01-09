using FlashSale.Api.DTOs;
using FlashSale.Core.Entities;
using FlashSale.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlashSale.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de pedidos.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStreamPublisher _streamPublisher;
    private readonly ILogger<OrdersController> _logger;

    private const string OrdersStream = "orders:pending";

    public OrdersController(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IStreamPublisher streamPublisher,
        ILogger<OrdersController> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _streamPublisher = streamPublisher;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo pedido.
    /// </summary>
    /// <remarks>
    /// O pedido é aceito e processado de forma assíncrona.
    /// Use o endpoint GET /orders/{id} para verificar o status.
    /// </remarks>
    /// <param name="request">Dados do pedido.</param>
    /// <returns>ID do pedido criado.</returns>
    /// <response code="202">Pedido aceito para processamento.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="409">Pedido duplicado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderAcceptedResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        _logger.LogInformation(
            "Recebendo pedido. CustomerId: {CustomerId}, IdempotencyKey: {IdempotencyKey}, Items: {ItemCount}",
            request.CustomerId, request.IdempotencyKey, request.Items.Count);

        // Verificar idempotência
        var existingOrder = await _orderRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey);
        if (existingOrder != null)
        {
            _logger.LogInformation(
                "Pedido duplicado detectado. IdempotencyKey: {IdempotencyKey}, ExistingOrderId: {OrderId}",
                request.IdempotencyKey, existingOrder.Id);

            return Accepted(new OrderAcceptedResponse
            {
                OrderId = existingOrder.Id,
                Status = existingOrder.Status.ToString(),
                Message = "Pedido já existe."
            });
        }

        // Criar pedido (será processado pelo Worker via Redis Streams)
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            IdempotencyKey = request.IdempotencyKey,
            UtmSource = request.UtmSource,
            UtmMedium = request.UtmMedium,
            UtmCampaign = request.UtmCampaign,
            Items = request.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };

        await _orderRepository.AddAsync(order);

        _logger.LogInformation("Pedido criado. OrderId: {OrderId}", order.Id);

        // Publicar no Redis Stream para processamento assíncrono pelo Worker
        var streamMessage = new OrderStreamMessage
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            IdempotencyKey = order.IdempotencyKey,
            Items = order.Items.Select(i => new OrderItemStreamMessage
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };

        await _streamPublisher.PublishAsync(OrdersStream, streamMessage);
        _logger.LogInformation("Pedido publicado no stream. OrderId: {OrderId}", order.Id);

        return Accepted(new OrderAcceptedResponse
        {
            OrderId = order.Id,
            Status = "Pending",
            Message = "Pedido recebido e será processado em instantes."
        });
    }

    /// <summary>
    /// Obtém o status de um pedido.
    /// </summary>
    /// <param name="id">ID do pedido.</param>
    /// <returns>Dados do pedido.</returns>
    /// <response code="200">Pedido encontrado.</response>
    /// <response code="404">Pedido não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound(new { message = $"Pedido {id} não encontrado" });
        }

        var response = new OrderResponse
        {
            Id = order.Id,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            ErrorMessage = order.ErrorMessage,
            CreatedAt = order.CreatedAt,
            ProcessedAt = order.ProcessedAt,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Obtém o status simplificado de um pedido.
    /// </summary>
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderStatus(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound(new { message = $"Pedido {id} não encontrado" });
        }

        return Ok(new
        {
            orderId = order.Id,
            status = order.Status.ToString(),
            processedAt = order.ProcessedAt
        });
    }
}
