namespace PalkinLib.Models;

/// <summary>
/// Продажа продукции партнеру (история реализации)
/// </summary>
public class Sale
{
    public int Id { get; set; }

    /// <summary>
    /// Внешний ключ на партнера
    /// </summary>
    public int PartnerId { get; set; }

    /// <summary>
    /// Наименование продукции
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Количество проданной продукции
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Дата продажи
    /// </summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// Сумма продажи в рублях
    /// </summary>
    public decimal Amount { get; set; }

    // Навигационное свойство (не используется при сохранении)
    public Partner? Partner { get; set; }
}
