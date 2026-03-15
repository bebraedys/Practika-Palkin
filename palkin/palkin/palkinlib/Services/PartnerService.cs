using PalkinLib.Models;
using PalkinLib.Repositories;

namespace PalkinLib.Services;

/// <summary>
/// Сервис для управления партнерами
/// </summary>
public interface IPartnerService
{
    Task<List<PartnerViewModel>> GetAllPartnersAsync();
    Task<PartnerViewModel?> GetPartnerByIdAsync(int id);
    Task<PartnerViewModel> AddPartnerAsync(PartnerViewModel partnerViewModel);
    Task UpdatePartnerAsync(PartnerViewModel partnerViewModel);
    Task DeletePartnerAsync(int id);
    Task<List<PartnerType>> GetPartnerTypesAsync();
    Task<List<Sale>> GetPartnerSalesAsync(int partnerId);
    Task<decimal> GetPartnerTotalSalesAsync(int partnerId);
    Task<SaleViewModel> AddSaleAsync(SaleViewModel saleViewModel);
    Task UpdateSaleAsync(SaleViewModel saleViewModel);
    Task DeleteSaleAsync(int saleId);
}

/// <summary>
/// ViewModel для отображения данных партнера
/// </summary>
public class PartnerViewModel
{
    public int Id { get; set; }
    public int PartnerTypeId { get; set; }
    public string PartnerTypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalAddress { get; set; }
    public string? Inn { get; set; }
    public string? DirectorName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public int DiscountPercent { get; set; }
    public string DiscountDescription { get; set; } = string.Empty;

    /// <summary>
    /// Преобразование из модели домена
    /// </summary>
    public static PartnerViewModel FromModel(Partner partner, decimal totalSales)
    {
        var discountPercent = DiscountService.CalculateDiscountPercent(totalSales);
        return new PartnerViewModel
        {
            Id = partner.Id,
            PartnerTypeId = partner.PartnerTypeId,
            PartnerTypeName = partner.PartnerType?.Name ?? string.Empty,
            Name = partner.Name,
            LegalAddress = partner.LegalAddress,
            Inn = partner.Inn,
            DirectorName = partner.DirectorName,
            Phone = partner.Phone,
            Email = partner.Email,
            Rating = partner.Rating,
            CreatedAt = partner.CreatedAt,
            UpdatedAt = partner.UpdatedAt,
            TotalSalesAmount = totalSales,
            DiscountPercent = discountPercent,
            DiscountDescription = DiscountService.GetDiscountDescription(totalSales)
        };
    }

    /// <summary>
    /// Преобразование в модель домена
    /// </summary>
    public Partner ToModel()
    {
        return new Partner
        {
            Id = Id,
            PartnerTypeId = PartnerTypeId,
            Name = Name,
            LegalAddress = LegalAddress,
            Inn = Inn,
            DirectorName = DirectorName,
            Phone = Phone,
            Email = Email,
            Rating = Rating
            // CreatedAt и UpdatedAt устанавливаются в репозитории
        };
    }
}

/// <summary>
/// Реализация сервиса управления партнерами
/// </summary>
public class PartnerService : IPartnerService
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly IPartnerTypeRepository _partnerTypeRepository;
    private readonly ISaleRepository _saleRepository;

    public PartnerService(
        IPartnerRepository partnerRepository,
        IPartnerTypeRepository partnerTypeRepository,
        ISaleRepository saleRepository)
    {
        _partnerRepository = partnerRepository;
        _partnerTypeRepository = partnerTypeRepository;
        _saleRepository = saleRepository;
    }

    public async Task<List<PartnerViewModel>> GetAllPartnersAsync()
    {
        var partners = await _partnerRepository.GetAllAsync();
        var result = new List<PartnerViewModel>();

        foreach (var partner in partners)
        {
            var totalSales = await _partnerRepository.GetTotalSalesByPartnerIdAsync(partner.Id);
            result.Add(PartnerViewModel.FromModel(partner, totalSales));
        }

        return result;
    }

    public async Task<PartnerViewModel?> GetPartnerByIdAsync(int id)
    {
        var partner = await _partnerRepository.GetByIdAsync(id);
        if (partner == null)
            return null;

        var totalSales = await _partnerRepository.GetTotalSalesByPartnerIdAsync(partner.Id);
        return PartnerViewModel.FromModel(partner, totalSales);
    }

    public async Task<PartnerViewModel> AddPartnerAsync(PartnerViewModel partnerViewModel)
    {
        var partner = partnerViewModel.ToModel();
        var createdPartner = await _partnerRepository.AddAsync(partner);
        
        var totalSales = await _partnerRepository.GetTotalSalesByPartnerIdAsync(createdPartner.Id);
        return PartnerViewModel.FromModel(createdPartner, totalSales);
    }

    public async Task UpdatePartnerAsync(PartnerViewModel partnerViewModel)
    {
        var partner = await _partnerRepository.GetByIdAsync(partnerViewModel.Id);
        if (partner == null)
            throw new Exception("Партнёр не найден");

        partner.Name = partnerViewModel.Name;
        partner.PartnerTypeId = partnerViewModel.PartnerTypeId;
        partner.LegalAddress = partnerViewModel.LegalAddress;
        partner.Inn = partnerViewModel.Inn;
        partner.DirectorName = partnerViewModel.DirectorName;
        partner.Phone = partnerViewModel.Phone;
        partner.Email = partnerViewModel.Email;
        partner.Rating = partnerViewModel.Rating;

        await _partnerRepository.UpdateAsync(partner);
    }

    public async Task DeletePartnerAsync(int id)
    {
        await _partnerRepository.DeleteAsync(id);
    }

    public async Task<List<PartnerType>> GetPartnerTypesAsync()
    {
        return await _partnerTypeRepository.GetAllAsync();
    }

    public async Task<List<Sale>> GetPartnerSalesAsync(int partnerId)
    {
        return await _saleRepository.GetByPartnerIdAsync(partnerId);
    }

    public async Task<decimal> GetPartnerTotalSalesAsync(int partnerId)
    {
        return await _partnerRepository.GetTotalSalesByPartnerIdAsync(partnerId);
    }

    public async Task<SaleViewModel> AddSaleAsync(SaleViewModel saleViewModel)
    {
        var sale = saleViewModel.ToModel();
        var createdSale = await _saleRepository.AddAsync(sale);

        return SaleViewModel.FromModel(createdSale);
    }

    public async Task UpdateSaleAsync(SaleViewModel saleViewModel)
    {
        var sale = saleViewModel.ToModel();
        await _saleRepository.UpdateAsync(sale);
    }

    public async Task DeleteSaleAsync(int saleId)
    {
        await _saleRepository.DeleteAsync(saleId);
    }
}

/// <summary>
/// ViewModel для отображения данных о продаже
/// </summary>
public class SaleViewModel
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal Amount { get; set; }

    /// <summary>
    /// Преобразование из модели домена
    /// </summary>
    public static SaleViewModel FromModel(Sale sale)
    {
        return new SaleViewModel
        {
            Id = sale.Id,
            PartnerId = sale.PartnerId,
            ProductName = sale.ProductName,
            Quantity = sale.Quantity,
            SaleDate = sale.SaleDate,
            Amount = sale.Amount
        };
    }

    /// <summary>
    /// Преобразование в модель домена
    /// </summary>
    public Sale ToModel()
    {
        return new Sale
        {
            Id = Id,
            PartnerId = PartnerId,
            ProductName = ProductName,
            Quantity = Quantity,
            SaleDate = SaleDate,
            Amount = Amount
        };
    }
}
