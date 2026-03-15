namespace PalkinLib.Models;

/// <summary>
/// Тип партнера (розничный, оптовый, интернет-магазин и т.д.)
/// </summary>
public class PartnerType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Навигационное свойство
    public virtual ICollection<Partner> Partners { get; set; } = new List<Partner>();
}
