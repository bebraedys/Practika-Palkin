using PalkinLib.Models;

namespace PalkinLib.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с типами партнеров
/// </summary>
public interface IPartnerTypeRepository
{
    Task<List<PartnerType>> GetAllAsync();
    Task<PartnerType?> GetByIdAsync(int id);
    Task<PartnerType> AddAsync(PartnerType partnerType);
    Task UpdateAsync(PartnerType partnerType);
    Task DeleteAsync(int id);
}

/// <summary>
/// Интерфейс репозитория для работы с партнерами
/// </summary>
public interface IPartnerRepository
{
    Task<List<Partner>> GetAllAsync();
    Task<Partner?> GetByIdAsync(int id);
    Task<Partner> AddAsync(Partner partner);
    Task UpdateAsync(Partner partner);
    Task DeleteAsync(int id);
    Task<decimal> GetTotalSalesByPartnerIdAsync(int partnerId);
}

/// <summary>
/// Интерфейс репозитория для работы с продажами
/// </summary>
public interface ISaleRepository
{
    Task<List<Sale>> GetByPartnerIdAsync(int partnerId);
    Task<Sale> AddAsync(Sale sale);
    Task UpdateAsync(Sale sale);
    Task DeleteAsync(int id);
}
