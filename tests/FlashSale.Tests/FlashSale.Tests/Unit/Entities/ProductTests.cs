using FlashSale.Core.Entities;
using FluentAssertions;

namespace FlashSale.Tests.Unit.Entities;

/// <summary>
/// Testes unitários para a entidade Product.
/// Segue padrão AAA (Arrange, Act, Assert) conforme 06-TESTING.md.
/// </summary>
[Trait("Category", "Unit")]
public class ProductTests
{
    // ═══════════════════════════════════════════════════════════════════════
    // HasSufficientStock
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void HasSufficientStock_WhenStockIsGreaterThanQuantity_ShouldReturnTrue()
    {
        // Arrange
        var product = new Product { Stock = 10 };

        // Act
        var result = product.HasSufficientStock(5);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasSufficientStock_WhenStockEqualsQuantity_ShouldReturnTrue()
    {
        // Arrange
        var product = new Product { Stock = 5 };

        // Act
        var result = product.HasSufficientStock(5);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasSufficientStock_WhenStockIsLessThanQuantity_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product { Stock = 3 };

        // Act
        var result = product.HasSufficientStock(5);

        // Assert
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DecrementStock
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void DecrementStock_WhenSufficientStock_ShouldDecrementAndIncrementVersion()
    {
        // Arrange
        var product = new Product { Stock = 10, Version = 1 };

        // Act
        product.DecrementStock(3);

        // Assert
        product.Stock.Should().Be(7);
        product.Version.Should().Be(2);
    }

    [Fact]
    public void DecrementStock_WhenInsufficientStock_ShouldThrowException()
    {
        // Arrange
        var product = new Product { Stock = 2 };

        // Act
        var act = () => product.DecrementStock(5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Estoque insuficiente*");
    }

    [Fact]
    public void DecrementStock_WhenExactStock_ShouldDecrementToZero()
    {
        // Arrange
        var product = new Product { Stock = 5, Version = 0 };

        // Act
        product.DecrementStock(5);

        // Assert
        product.Stock.Should().Be(0);
        product.Version.Should().Be(1);
    }
}
