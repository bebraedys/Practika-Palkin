using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PalkinLib.Models;
using PalkinLib.Services;

namespace palkinprog.Dialogs;

/// <summary>
/// Окно просмотра истории продаж партнера
/// </summary>
public partial class SalesHistoryWindow : Window
{
    private readonly string _partnerName;
    private readonly int _partnerId;
    private readonly IPartnerService _partnerService;
    private List<Sale> _sales = new();
    private decimal _totalSales;
    private int _discountPercent;

    public SalesHistoryWindow(string partnerName, int partnerId, List<Sale> sales, decimal totalSales, int discountPercent, IPartnerService partnerService)
    {
        InitializeComponent();
        _partnerName = partnerName;
        _partnerId = partnerId;
        _sales = sales;
        _totalSales = totalSales;
        _discountPercent = discountPercent;
        _partnerService = partnerService;

        Title = $"История продаж - {partnerName}";

        InitializeData(partnerName, sales, totalSales, discountPercent);
    }

    private void InitializeData(string partnerName, List<Sale> sales, decimal totalSales, int discountPercent)
    {
        // Основная информация
        PartnerNameText.Text = partnerName;
        SalesCountText.Text = $"Всего продаж: {sales.Count}";
        DiscountText.Text = $"{discountPercent}%";
        TotalAmountText.Text = $"{totalSales:N2} ₽";

        // Расчет суммы скидки
        var discountAmount = DiscountService.CalculateDiscountAmount(totalSales);
        DiscountAmountText.Text = $"{discountAmount:N2} ₽";

        // Определение следующего уровня скидки
        NextLevelText.Text = GetNextLevelInfo(totalSales, discountPercent);

        // Заполнение таблицы
        SalesDataGrid.ItemsSource = sales;
    }

    /// <summary>
    /// Получение информации о следующем уровне скидки
    /// </summary>
    private static string GetNextLevelInfo(decimal totalSales, int currentDiscount)
    {
        if (currentDiscount >= 15)
            return "Максимальная скидка";

        return currentDiscount switch
        {
            0 => "10 000 ₽ (скидка 5%)",
            5 => "50 000 ₽ (скидка 10%)",
            10 => "300 000 ₽ (скидка 15%)",
            _ => "-"
        };
    }

    /// <summary>
    /// Загрузка данных о продажах
    /// </summary>
    private async Task LoadSalesAsync()
    {
        try
        {
            _sales = await _partnerService.GetPartnerSalesAsync(_partnerId);
            _totalSales = await _partnerService.GetPartnerTotalSalesAsync(_partnerId);
            _discountPercent = DiscountService.CalculateDiscountPercent(_totalSales);

            InitializeData(_partnerName, _sales, _totalSales, _discountPercent);
        }
        catch (Exception ex)
        {
            var innerException = ex.InnerException?.Message ?? "Нет внутреннего исключения";
            var fullError = $"Не удалось загрузить данные о продажах.\n\nОшибка: {ex.Message}\n\nВнутренняя ошибка: {innerException}";
            MessageBox.Show(
                fullError,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Добавление новой продажи
    /// </summary>
    private async void AddSaleClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var editWindow = new SaleEditWindow(null, _partnerId, _partnerService);
            editWindow.Owner = this;

            if (editWindow.ShowDialog() == true)
            {
                await LoadSalesAsync();
            }
        }
        catch (Exception ex)
        {
            var innerException = ex.InnerException?.Message ?? "Нет внутреннего исключения";
            var fullError = $"Не удалось добавить продажу.\n\nОшибка: {ex.Message}\n\nВнутренняя ошибка: {innerException}";
            MessageBox.Show(
                fullError,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Редактирование продажи
    /// </summary>
    private async void EditSaleClick(object sender, RoutedEventArgs e)
    {
        if (SalesDataGrid.SelectedItem is not Sale selectedSale)
        {
            MessageBox.Show(
                "Выберите продажу для редактирования.",
                "Предупреждение",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            var saleViewModel = SaleViewModel.FromModel(selectedSale);
            var editWindow = new SaleEditWindow(saleViewModel, _partnerId, _partnerService);
            editWindow.Owner = this;

            if (editWindow.ShowDialog() == true)
            {
                await LoadSalesAsync();
            }
        }
        catch (Exception ex)
        {
            var innerException = ex.InnerException?.Message ?? "Нет внутреннего исключения";
            var fullError = $"Не удалось редактировать продажу.\n\nОшибка: {ex.Message}\n\nВнутренняя ошибка: {innerException}";
            MessageBox.Show(
                fullError,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Удаление продажи
    /// </summary>
    private async void DeleteSaleClick(object sender, RoutedEventArgs e)
    {
        if (SalesDataGrid.SelectedItem is not Sale selectedSale)
        {
            MessageBox.Show(
                "Выберите продажу для удаления.",
                "Предупреждение",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Вы действительно хотите удалить продажу?\n\n" +
            $"Продукция: {selectedSale.ProductName}\n" +
            $"Сумма: {selectedSale.Amount:N2} ₽\n\n" +
            $"Это действие необратимо!",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _partnerService.DeleteSaleAsync(selectedSale.Id);
                await LoadSalesAsync();
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException?.Message ?? "Нет внутреннего исключения";
                var fullError = $"Не удалось удалить продажу.\n\nОшибка: {ex.Message}\n\nВнутренняя ошибка: {innerException}";
                MessageBox.Show(
                    fullError,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Обновление списка продаж
    /// </summary>
    private async void RefreshClick(object sender, RoutedEventArgs e)
    {
        await LoadSalesAsync();
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
