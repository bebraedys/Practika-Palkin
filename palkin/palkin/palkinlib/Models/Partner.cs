namespace PalkinLib.Models;

/// <summary>
/// Партнер компании - организация, реализующая продукцию
/// </summary>
public class Partner
{
    public int Id { get; set; }
    
    /// <summary>
    /// Внешний ключ на тип партнера
    /// </summary>
    public int PartnerTypeId { get; set; }
    
    /// <summary>
    /// Наименование компании
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Юридический адрес
    /// </summary>
    public string? LegalAddress { get; set; }
    
    /// <summary>
    /// ИНН организации
    /// </summary>
    public string? Inn { get; set; }
    
    /// <summary>
    /// ФИО директора
    /// </summary>
    public string? DirectorName { get; set; }
    
    /// <summary>
    /// Контактный телефон
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// Электронная почта
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Рейтинг партнера (неотрицательное целое число)
    /// </summary>
    public int Rating { get; set; }
    
    /// <summary>
    /// Дата создания записи
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата последнего изменения
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Навигационные свойства (не используются при сохранении)
    public PartnerType? PartnerType { get; set; }
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();

    /// <summary>
    /// Общая сумма продаж партнера за весь период
    /// </summary>
    public decimal TotalSalesAmount { get; set; }

    /// <summary>
    /// Расчет скидки на основе объема продаж
    /// до 10000 – 0%, от 10000 – до 50000 – 5%, от 50000 – до 300000 – 10%, более 300000 – 15%
    /// </summary>
    public int GetDiscountPercent()
    {
        if (TotalSalesAmount >= 300000)
            return 15;
        if (TotalSalesAmount >= 50000)
            return 10;
        if (TotalSalesAmount >= 10000)
            return 5;
        return 0;
    }
}
