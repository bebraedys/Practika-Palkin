using Microsoft.EntityFrameworkCore;
using PalkinLib.Data;
using PalkinLib.Models;

namespace PalkinLib.Repositories;

/// <summary>
/// Репозиторий для работы с типами партнеров
/// </summary>
public class PartnerTypeRepository : IPartnerTypeRepository
{
    private readonly Func<PalkinDbContext> _contextFactory;

    public PartnerTypeRepository(Func<PalkinDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<PartnerType>> GetAllAsync()
    {
        await using var context = _contextFactory();
        return await context.PartnerTypes.ToListAsync();
    }

    public async Task<PartnerType?> GetByIdAsync(int id)
    {
        await using var context = _contextFactory();
        return await context.PartnerTypes.FindAsync(id);
    }

    public async Task<PartnerType> AddAsync(PartnerType partnerType)
    {
        await using var context = _contextFactory();
        await context.PartnerTypes.AddAsync(partnerType);
        await context.SaveChangesAsync();
        return partnerType;
    }

    public async Task UpdateAsync(PartnerType partnerType)
    {
        await using var context = _contextFactory();
        context.PartnerTypes.Update(partnerType);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var context = _contextFactory();
        var partnerType = await context.PartnerTypes.FindAsync(id);
        if (partnerType != null)
        {
            context.PartnerTypes.Remove(partnerType);
            await context.SaveChangesAsync();
        }
    }
}
