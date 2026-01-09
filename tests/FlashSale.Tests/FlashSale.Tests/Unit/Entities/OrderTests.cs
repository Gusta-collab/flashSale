using FlashSale.Core.Entities;
using FlashSale.Core.Enums;
using FluentAssertions;

namespace FlashSale.Tests.Unit.Entities;

/// <summary>
/// Testes unitários para a entidade Order.
/// </summary>
[Trait("Category", "Unit")]
public class OrderTests
{
    // ═══════════════════════════════════════════════════════════════════════
    // Confirm
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Confirm_ShouldSetStatusToConfirmedAndSetProcessedAt()
    {
        // Arrange
        var order = new Order { Status = OrderStatus.Pending };

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ProcessedAt.Should().NotBeNull();
        order.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Fail
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Fail_ShouldSetStatusToFailedAndSetErrorMessage()
    {
        // Arrange
        var order = new Order { Status = OrderStatus.Pending };
        var reason = "Estoque insuficiente";

        // Act
        order.Fail(reason);

        // Assert
        order.Status.Should().Be(OrderStatus.Failed);
        order.ErrorMessage.Should().Be(reason);
        order.ProcessedAt.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // CalculateTotal
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void CalculateTotal_WithMultipleItems_ShouldSumCorrectly()
    {
        // Arrange
        var order = new Order
        {
            Items = new List<OrderItem>
            {
                new() { Quantity = 2, UnitPrice = 10.00m },
                new() { Quantity = 3, UnitPrice = 5.50m }
            }
        };

        // Act
        order.CalculateTotal();

        // Assert
        order.TotalAmount.Should().Be(36.50m); // (2 * 10) + (3 * 5.5)
    }

    [Fact]
    public void CalculateTotal_WithNoItems_ShouldBeZero()
    {
        // Arrange
        var order = new Order { Items = new List<OrderItem>() };

        // Act
        order.CalculateTotal();

        // Assert
        order.TotalAmount.Should().Be(0);
    }
}
