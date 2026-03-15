using PalkinLib.Models;

namespace PalkinLib.Services;

/// <summary>
/// Сервис для расчета скидок партнерам
/// Скидка зависит от общего количества реализованной партнером продукции:
/// до 10000 – 0%, от 10000 – до 50000 – 5%, от 50000 – до 300000 – 10%, более 300000 – 15%
/// </summary>
public static class DiscountService
{
    /// <summary>
    /// Расчет процента скидки на основе общей суммы продаж
    /// </summary>
    /// <param name="totalSalesAmount">Общая сумма продаж в рублях</param>
    /// <returns>Процент скидки (0, 5, 10 или 15)</returns>
    public static int CalculateDiscountPercent(decimal totalSalesAmount)
    {
        if (totalSalesAmount >= 300000)
            return 15;
        if (totalSalesAmount >= 50000)
            return 10;
        if (totalSalesAmount >= 10000)
            return 5;
        return 0;
    }

    /// <summary>
    /// Расчет суммы скидки
    /// </summary>
    /// <param name="totalSalesAmount">Общая сумма продаж в рублях</param>
    /// <returns>Сумма скидки в рублях</returns>
    public static decimal CalculateDiscountAmount(decimal totalSalesAmount)
    {
        var percent = CalculateDiscountPercent(totalSalesAmount);
        return totalSalesAmount * percent / 100;
    }

    /// <summary>
    /// Получение текстового описания скидки
    /// </summary>
    /// <param name="totalSalesAmount">Общая сумма продаж в рублях</param>
    /// <returns>Текстовое описание скидки</returns>
    public static string GetDiscountDescription(decimal totalSalesAmount)
    {
        var percent = CalculateDiscountPercent(totalSalesAmount);
        return percent switch
        {
            15 => "15% (Премиум партнер)",
            10 => "10% (Золотой партнер)",
            5 => "5% (Серебряный партнер)",
            _ => "0% (Стандарт)"
        };
    }
}
