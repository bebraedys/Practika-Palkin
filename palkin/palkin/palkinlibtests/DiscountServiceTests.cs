using PalkinLib.Services;

namespace PalkinLibTests;

/// <summary>
/// Тесты для сервиса расчета скидок
/// </summary>
public class DiscountServiceTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(5000, 0)]
    [InlineData(9999, 0)]
    public void CalculateDiscountPercent_ReturnsZero_WhenSalesBelow10000(decimal sales, int expectedDiscount)
    {
        // Act
        var result = DiscountService.CalculateDiscountPercent(sales);

        // Assert
        Assert.Equal(expectedDiscount, result);
    }

    [Theory]
    [InlineData(10000, 5)]
    [InlineData(25000, 5)]
    [InlineData(49999, 5)]
    public void CalculateDiscountPercent_ReturnsFive_WhenSalesBetween10000And50000(decimal sales, int expectedDiscount)
    {
        // Act
        var result = DiscountService.CalculateDiscountPercent(sales);

        // Assert
        Assert.Equal(expectedDiscount, result);
    }

    [Theory]
    [InlineData(50000, 10)]
    [InlineData(150000, 10)]
    [InlineData(299999, 10)]
    public void CalculateDiscountPercent_ReturnsTen_WhenSalesBetween50000And300000(decimal sales, int expectedDiscount)
    {
        // Act
        var result = DiscountService.CalculateDiscountPercent(sales);

        // Assert
        Assert.Equal(expectedDiscount, result);
    }

    [Theory]
    [InlineData(300000, 15)]
    [InlineData(500000, 15)]
    [InlineData(1000000, 15)]
    public void CalculateDiscountPercent_ReturnsFifteen_WhenSalesAbove300000(decimal sales, int expectedDiscount)
    {
        // Act
        var result = DiscountService.CalculateDiscountPercent(sales);

        // Assert
        Assert.Equal(expectedDiscount, result);
    }

    [Fact]
    public void CalculateDiscountAmount_CalculatesCorrectly()
    {
        // Arrange
        var sales = 100000m; // 10% discount = 10000

        // Act
        var result = DiscountService.CalculateDiscountAmount(sales);

        // Assert
        Assert.Equal(10000m, result);
    }

    [Theory]
    [InlineData(0, "0% (Стандарт)")]
    [InlineData(5000, "0% (Стандарт)")]
    [InlineData(10000, "5% (Серебряный партнер)")]
    [InlineData(50000, "10% (Золотой партнер)")]
    [InlineData(300000, "15% (Премиум партнер)")]
    public void GetDiscountDescription_ReturnsCorrectDescription(decimal sales, string expectedDescription)
    {
        // Act
        var result = DiscountService.GetDiscountDescription(sales);

        // Assert
        Assert.Equal(expectedDescription, result);
    }
}
