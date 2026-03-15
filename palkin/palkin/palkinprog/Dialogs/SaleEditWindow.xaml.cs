using System.Globalization;
using System.Windows;
using PalkinLib.Services;

namespace palkinprog.Dialogs;

/// <summary>
/// Окно добавления/редактирования продажи
/// </summary>
public partial class SaleEditWindow : Window
{
    private readonly SaleViewModel? _existingSale;
    private readonly int _partnerId;
    private readonly IPartnerService _partnerService;

    public SaleEditWindow(SaleViewModel? sale, int partnerId, IPartnerService partnerService)
    {
        InitializeComponent();
        _existingSale = sale;
        _partnerId = partnerId;
        _partnerService = partnerService;

        Title = sale == null ? "Добавление продажи" : "Редактирование продажи";
        TitleText.Text = sale == null ? "Добавление продажи" : "Редактирование продажи";

        InitializeForm();
    }

    private void InitializeForm()
    {
        if (_existingSale != null)
        {
            // Заполнение полей данными существующей продажи
            ProductNameTextBox.Text = _existingSale.ProductName;
            QuantityTextBox.Text = _existingSale.Quantity.ToString();
            SaleDatePicker.SelectedDate = _existingSale.SaleDate;
            AmountTextBox.Text = _existingSale.Amount.ToString("N2");
        }
        else
        {
            // Установка текущей даты по умолчанию
            SaleDatePicker.SelectedDate = DateTime.Today;
        }
    }

    /// <summary>
    /// Валидация данных формы
    /// </summary>
    private bool ValidateForm(out string errorMessage)
    {
        errorMessage = string.Empty;

        // Проверка наименования продукции
        if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
        {
            errorMessage = "Наименование продукции является обязательным полем.";
            ProductNameTextBox.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            return false;
        }
        ProductNameTextBox.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 209, 209, 209));

        // Проверка количества
        if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
        {
            QuantityErrorText.Text = "Количество должно быть положительным целым числом.";
            QuantityErrorText.Visibility = Visibility.Visible;
            QuantityTextBox.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            errorMessage = "Количество должно быть положительным целым числом.";
            return false;
        }
        QuantityErrorText.Visibility = Visibility.Collapsed;
        QuantityTextBox.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 209, 209, 209));

        // Проверка даты
        if (SaleDatePicker.SelectedDate == null)
        {
            errorMessage = "Дата продажи является обязательным полем.";
            return false;
        }

        // Проверка суммы
        if (!decimal.TryParse(AmountTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal amount) || amount < 0)
        {
            AmountErrorText.Text = "Сумма должна быть неотрицательным числом.";
            AmountErrorText.Visibility = Visibility.Visible;
            AmountTextBox.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            errorMessage = "Сумма должна быть неотрицательным числом.";
            return false;
        }
        AmountErrorText.Visibility = Visibility.Collapsed;
        AmountTextBox.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 209, 209, 209));

        return true;
    }

    /// <summary>
    /// Сохранение данных продажи
    /// </summary>
    private async void SaveClick(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm(out string errorMessage))
        {
            MessageBox.Show(
                errorMessage,
                "Ошибка валидации",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (_existingSale != null)
            {
                // Обновление существующей продажи
                _existingSale.ProductName = ProductNameTextBox.Text.Trim();
                _existingSale.Quantity = int.Parse(QuantityTextBox.Text);
                _existingSale.SaleDate = SaleDatePicker.SelectedDate!.Value.Date;
                _existingSale.Amount = decimal.Parse(AmountTextBox.Text, CultureInfo.CurrentCulture);

                await _partnerService.UpdateSaleAsync(_existingSale);
            }
            else
            {
                // Добавление новой продажи
                var newSale = new SaleViewModel
                {
                    PartnerId = _partnerId,
                    ProductName = ProductNameTextBox.Text.Trim(),
                    Quantity = int.Parse(QuantityTextBox.Text),
                    SaleDate = SaleDatePicker.SelectedDate!.Value.Date,
                    Amount = decimal.Parse(AmountTextBox.Text, CultureInfo.CurrentCulture)
                };

                await _partnerService.AddSaleAsync(newSale);
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            var innerException = ex.InnerException?.Message ?? "Нет внутреннего исключения";
            var fullError = $"Произошла ошибка при сохранении данных.\n\nОшибка: {ex.Message}\n\nВнутренняя ошибка: {innerException}";
            MessageBox.Show(
                fullError,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Отмена редактирования
    /// </summary>
    private void CancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
