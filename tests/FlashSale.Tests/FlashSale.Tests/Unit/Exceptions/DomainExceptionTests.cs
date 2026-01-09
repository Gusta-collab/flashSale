using FlashSale.Core.Exceptions;
using FluentAssertions;

namespace FlashSale.Tests.Unit.Exceptions;

/// <summary>
/// Testes unitários para exceções de domínio.
/// </summary>
[Trait("Category", "Unit")]
public class DomainExceptionTests
{
    [Fact]
    public void InsufficientStockException_ShouldContainProductIdAndQuantities()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var requested = 10;
        var available = 5;

        // Act
        var exception = new InsufficientStockException(productId, requested, available);

        // Assert
        exception.ProductId.Should().Be(productId);
        exception.RequestedQuantity.Should().Be(requested);
        exception.AvailableStock.Should().Be(available);
        exception.Message.Should().Contain(productId.ToString());
    }

    [Fact]
    public void OrderNotFoundException_ShouldContainOrderId()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var exception = new OrderNotFoundException(orderId);

        // Assert
        exception.OrderId.Should().Be(orderId);
        exception.Message.Should().Contain(orderId.ToString());
    }

    [Fact]
    public void DuplicateOrderException_ShouldContainIdempotencyKey()
    {
        // Arrange
        var idempotencyKey = "test-key-123";
        var existingOrderId = Guid.NewGuid();

        // Act
        var exception = new DuplicateOrderException(idempotencyKey, existingOrderId);

        // Assert
        exception.IdempotencyKey.Should().Be(idempotencyKey);
        exception.ExistingOrderId.Should().Be(existingOrderId);
    }
}
