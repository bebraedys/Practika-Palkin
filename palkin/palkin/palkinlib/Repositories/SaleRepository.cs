using Microsoft.EntityFrameworkCore;
using PalkinLib.Data;
using PalkinLib.Models;

namespace PalkinLib.Repositories;

/// <summary>
/// Репозиторий для работы с продажами
/// </summary>
public class SaleRepository : ISaleRepository
{
    private readonly Func<PalkinDbContext> _contextFactory;

    public SaleRepository(Func<PalkinDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Sale>> GetByPartnerIdAsync(int partnerId)
    {
        await using var context = _contextFactory();
        return await context.Sales
            .Where(s => s.PartnerId == partnerId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<Sale> AddAsync(Sale sale)
    {
        await using var context = _contextFactory();
        await context.Sales.AddAsync(sale);
        await context.SaveChangesAsync();
        return sale;
    }

    public async Task UpdateAsync(Sale sale)
    {
        await using var context = _contextFactory();
        context.Entry(sale).Property(e => e.SaleDate).IsModified = true;
        context.Sales.Update(sale);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var context = _contextFactory();
        var sale = await context.Sales.FindAsync(id);
        if (sale != null)
        {
            context.Sales.Remove(sale);
            await context.SaveChangesAsync();
        }
    }
}
