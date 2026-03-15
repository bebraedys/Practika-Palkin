using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PalkinLib.Services;
using palkinprog.Dialogs;

namespace palkinprog;


public partial class MainWindow : Window
{
    private readonly IPartnerService _partnerService;
    private List<PartnerViewModel> _partners = new();

    public MainWindow()
    {
        InitializeComponent();
        _partnerService = ((App)Application.Current).GetService<IPartnerService>()!;
        Title = "Мастер Пол - Управление партнерами";
        Loaded += MainWindowLoaded;
    }

    private async void MainWindowLoaded(object sender, RoutedEventArgs e)
    {
        await LoadPartnersAsync();
    }

  
    private async Task LoadPartnersAsync()
    {
        try
        {
            StatusText.Text = "Загрузка данных...";
            _partners = await _partnerService.GetAllPartnersAsync();
            PartnersDataGrid.ItemsSource = null;
            PartnersDataGrid.ItemsSource = _partners;
            StatusText.Text = $"Загружено партнеров: {_partners.Count}";
        }
        catch (Exception ex)
        {
            StatusText.Text = "Ошибка загрузки";
            MessageBox.Show(
                $"Не удалось загрузить список партнеров.\n\nОшибка: {ex.Message}\n\nПроверьте подключение к базе данных.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

   
    private async void AddPartnerClick(object sender, RoutedEventArgs e)
    {
        await OpenPartnerEditWindowAsync(null);
    }

   
    private async void EditPartnerClick(object sender, RoutedEventArgs e)
    {
        if (PartnersDataGrid.SelectedItem is PartnerViewModel selectedPartner)
        {
            await OpenPartnerEditWindowAsync(selectedPartner);
        }
    }

    
    private async void PartnersDataGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (PartnersDataGrid.SelectedItem is PartnerViewModel selectedPartner)
        {
            await OpenPartnerEditWindowAsync(selectedPartner);
        }
    }

   
    private async void ShowSalesHistoryClick(object sender, RoutedEventArgs e)
    {
        if (PartnersDataGrid.SelectedItem is not PartnerViewModel selectedPartner)
        {
            MessageBox.Show(
                "Выберите партнера для просмотра истории продаж.",
                "Предупреждение",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            var sales = await _partnerService.GetPartnerSalesAsync(selectedPartner.Id);
            var totalSales = await _partnerService.GetPartnerTotalSalesAsync(selectedPartner.Id);

            var salesWindow = new SalesHistoryWindow(
                selectedPartner.Name,
                selectedPartner.Id,
                sales,
                totalSales,
                selectedPartner.DiscountPercent,
                _partnerService);

            salesWindow.Owner = this;
            salesWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось загрузить историю продаж.\n\nОшибка: {ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

   
    private async void DeletePartnerClick(object sender, RoutedEventArgs e)
    {
        if (PartnersDataGrid.SelectedItem is not PartnerViewModel selectedPartner)
        {
            MessageBox.Show(
                "Выберите партнера для удаления.",
                "Предупреждение",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Вы действительно хотите удалить партнера \"{selectedPartner.Name}\"?\n\n" +
            $"Все связанные записи о продажах также будут удалены.\n\n" +
            $"Это действие необратимо!",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _partnerService.DeletePartnerAsync(selectedPartner.Id);
                StatusText.Text = $"Партнер \"{selectedPartner.Name}\" удален";
                await LoadPartnersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось удалить партнера.\n\nОшибка: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

  
    private async void RefreshClick(object sender, RoutedEventArgs e)
    {
        await LoadPartnersAsync();
    }

   
    private async Task OpenPartnerEditWindowAsync(PartnerViewModel? partner)
    {
        try
        {
            var partnerTypes = await _partnerService.GetPartnerTypesAsync();
            var editWindow = new PartnerEditWindow(partner, partnerTypes, _partnerService);
            editWindow.Owner = this;

            if (editWindow.ShowDialog() == true)
            {
                await LoadPartnersAsync();
                StatusText.Text = partner == null
                    ? "Партнер добавлен"
                    : $"Партнер \"{partner.Name}\" обновлен";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось открыть форму редактирования.\n\nОшибка: {ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
