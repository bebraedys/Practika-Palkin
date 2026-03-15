using Microsoft.EntityFrameworkCore;
using PalkinLib.Models;

namespace PalkinLib.Data;

/// <summary>
/// Контекст базы данных для подсистемы работы с партнерами
/// </summary>
public class PalkinDbContext : DbContext
{
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<PartnerType> PartnerTypes => Set<PartnerType>();
    public DbSet<Sale> Sales => Set<Sale>();

    private readonly string _connectionString;

    public PalkinDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка схемы
        modelBuilder.HasDefaultSchema("app");

        // Конфигурация PartnerType
        modelBuilder.Entity<PartnerType>(entity =>
        {
            entity.ToTable("partner_types");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100).HasColumnName("name");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Конфигурация Partner
        modelBuilder.Entity<Partner>(entity =>
        {
            entity.ToTable("partners");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.LegalAddress).HasMaxLength(500).HasColumnName("legal_address");
            entity.Property(e => e.Inn).HasMaxLength(20).HasColumnName("inn");
            entity.Property(e => e.DirectorName).HasMaxLength(255).HasColumnName("director_name");
            entity.Property(e => e.Phone).HasMaxLength(50).HasColumnName("phone");
            entity.Property(e => e.Email).HasMaxLength(100).HasColumnName("email");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone").HasColumnName("updated_at");
            entity.Property(e => e.PartnerTypeId).HasColumnName("partner_type_id");

            // Значения устанавливаются только из кода C#, а не базой данных
            entity.Property(e => e.CreatedAt).ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).ValueGeneratedOnAddOrUpdate();

            entity.HasOne(e => e.PartnerType)
                  .WithMany(e => e.Partners)
                  .HasForeignKey(e => e.PartnerTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Игнорируем вычисляемое свойство и навигационные свойства при маппинге
            entity.Ignore(e => e.TotalSalesAmount);

            // Ограничение проверки для рейтинга
            entity.HasCheckConstraint("CK_Partners_Rating", "\"rating\" >= 0");
        });

        // Конфигурация Sale
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.ToTable("sales");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PartnerId).HasColumnName("partner_id");
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(255).HasColumnName("product_name");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SaleDate).HasColumnType("date").HasColumnName("sale_date");
            entity.Property(e => e.Amount).HasColumnName("amount");

            entity.HasOne(e => e.Partner)
                  .WithMany(e => e.Sales)
                  .HasForeignKey(e => e.PartnerId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Ограничения проверки
            entity.HasCheckConstraint("CK_Sales_Quantity", "\"quantity\" > 0");
            entity.HasCheckConstraint("CK_Sales_Amount", "\"amount\" >= 0");
        });
    }
}
