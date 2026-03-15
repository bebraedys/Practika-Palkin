using Microsoft.EntityFrameworkCore;
using PalkinLib.Data;
using PalkinLib.Models;

namespace PalkinLib.Repositories;

/// <summary>
/// Репозиторий для работы с партнерами
/// </summary>
public class PartnerRepository : IPartnerRepository
{
    private readonly Func<PalkinDbContext> _contextFactory;

    public PartnerRepository(Func<PalkinDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Partner>> GetAllAsync()
    {
        await using var context = _contextFactory();
        return await context.Partners
            .Include(p => p.PartnerType)
            .ToListAsync();
    }

    public async Task<Partner?> GetByIdAsync(int id)
    {
        await using var context = _contextFactory();
        return await context.Partners
            .Include(p => p.PartnerType)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Partner> AddAsync(Partner partner)
    {
        await using var context = _contextFactory();
        var now = DateTime.Now;
        partner.CreatedAt = now;
        partner.UpdatedAt = now;
        await context.Partners.AddAsync(partner);
        await context.SaveChangesAsync();
        return partner;
    }

    public async Task UpdateAsync(Partner partner)
    {
        await using var context = _contextFactory();
        context.Entry(partner).Property(e => e.UpdatedAt).IsModified = true;
        context.Partners.Update(partner);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var context = _contextFactory();
        var partner = await context.Partners.FindAsync(id);
        if (partner != null)
        {
            context.Partners.Remove(partner);
            await context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetTotalSalesByPartnerIdAsync(int partnerId)
    {
        await using var context = _contextFactory();
        return await context.Sales
            .Where(s => s.PartnerId == partnerId)
            .SumAsync(s => s.Amount);
    }
}
