public static class DiscountService
{
    public static int CalculateDiscountPercent(decimal totalSales) => totalSales switch
    {
        >= 300_000 => 15,
        >= 50_000 => 10,
        >= 10_000 => 5,
        _ => 0
    };
}
