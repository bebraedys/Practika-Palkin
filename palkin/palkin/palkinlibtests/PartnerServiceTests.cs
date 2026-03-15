using Moq;
using PalkinLib.Models;
using PalkinLib.Repositories;
using PalkinLib.Services;

namespace PalkinLibTests;

/// <summary>
/// Тесты для сервиса управления партнерами
/// </summary>
public class PartnerServiceTests
{
    private readonly Mock<IPartnerRepository> _partnerRepositoryMock;
    private readonly Mock<IPartnerTypeRepository> _partnerTypeRepositoryMock;
    private readonly Mock<ISaleRepository> _saleRepositoryMock;
    private readonly PartnerService _partnerService;

    public PartnerServiceTests()
    {
        _partnerRepositoryMock = new Mock<IPartnerRepository>();
        _partnerTypeRepositoryMock = new Mock<IPartnerTypeRepository>();
        _saleRepositoryMock = new Mock<ISaleRepository>();
        
        _partnerService = new PartnerService(
            _partnerRepositoryMock.Object,
            _partnerTypeRepositoryMock.Object,
            _saleRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllPartnersAsync_ReturnsViewModelsWithTotalSales()
    {
        // Arrange
        var partnerType = new PartnerType { Id = 1, Name = "Розничный магазин" };
        var partners = new List<Partner>
        {
            new() { Id = 1, Name = "Partner 1", PartnerTypeId = 1, PartnerType = partnerType, Rating = 0 },
            new() { Id = 2, Name = "Partner 2", PartnerTypeId = 1, PartnerType = partnerType, Rating = 5 }
        };

        _partnerRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(partners);
        _partnerRepositoryMock.Setup(r => r.GetTotalSalesByPartnerIdAsync(1)).ReturnsAsync(50000m);
        _partnerRepositoryMock.Setup(r => r.GetTotalSalesByPartnerIdAsync(2)).ReturnsAsync(100000m);

        // Act
        var result = await _partnerService.GetAllPartnersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Partner 1", result[0].Name);
        Assert.Equal("Partner 2", result[1].Name);
        Assert.Equal(50000m, result[0].TotalSalesAmount);
        Assert.Equal(100000m, result[1].TotalSalesAmount);
        Assert.Equal(10, result[1].DiscountPercent); // 10% for 100000
    }

    [Fact]
    public async Task GetPartnerByIdAsync_ReturnsNull_WhenPartnerNotFound()
    {
        // Arrange
        _partnerRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Partner?)null);

        // Act
        var result = await _partnerService.GetPartnerByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPartnerByIdAsync_ReturnsViewModel_WhenPartnerExists()
    {
        // Arrange
        var partnerType = new PartnerType { Id = 1, Name = "Оптовый магазин" };
        var partner = new Partner 
        { 
            Id = 1, 
            Name = "Test Partner", 
            PartnerTypeId = 1, 
            PartnerType = partnerType,
            Rating = 10 
        };

        _partnerRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(partner);
        _partnerRepositoryMock.Setup(r => r.GetTotalSalesByPartnerIdAsync(1)).ReturnsAsync(25000m);

        // Act
        var result = await _partnerService.GetPartnerByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Partner", result.Name);
        Assert.Equal(25000m, result.TotalSalesAmount);
        Assert.Equal(5, result.DiscountPercent);
    }

    [Fact]
    public async Task AddPartnerAsync_CallsRepository_AddsPartner()
    {
        // Arrange
        var viewModel = new PartnerViewModel
        {
            Name = "New Partner",
            PartnerTypeId = 1,
            PartnerTypeName = "Retail",
            Rating = 0
        };

        var savedPartner = new Partner { Id = 1, Name = "New Partner", PartnerTypeId = 1, Rating = 0 };
        _partnerRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Partner>())).ReturnsAsync(savedPartner);
        _partnerRepositoryMock.Setup(r => r.GetTotalSalesByPartnerIdAsync(1)).ReturnsAsync(0m);

        // Act
        var result = await _partnerService.AddPartnerAsync(viewModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Partner", result.Name);
        _partnerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Partner>()), Times.Once);
    }

    [Fact]
    public async Task DeletePartnerAsync_CallsRepository_Delete()
    {
        // Act
        await _partnerService.DeletePartnerAsync(1);

        // Assert
        _partnerRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetPartnerTypesAsync_ReturnsAllTypes()
    {
        // Arrange
        var types = new List<PartnerType>
        {
            new() { Id = 1, Name = "Розничный магазин" },
            new() { Id = 2, Name = "Оптовый магазин" }
        };
        _partnerTypeRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(types);

        // Act
        var result = await _partnerService.GetPartnerTypesAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetPartnerSalesAsync_ReturnsSalesForPartner()
    {
        // Arrange
        var sales = new List<Sale>
        {
            new() { Id = 1, PartnerId = 1, ProductName = "Product 1", Quantity = 10, Amount = 5000m },
            new() { Id = 2, PartnerId = 1, ProductName = "Product 2", Quantity = 5, Amount = 3000m }
        };
        _saleRepositoryMock.Setup(r => r.GetByPartnerIdAsync(1)).ReturnsAsync(sales);

        // Act
        var result = await _partnerService.GetPartnerSalesAsync(1);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].PartnerId);
    }
}
