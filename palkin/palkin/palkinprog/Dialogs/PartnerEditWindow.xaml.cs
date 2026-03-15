using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using PalkinLib.Models;
using PalkinLib.Services;

namespace palkinprog.Dialogs;

/// <summary>
/// Окно добавления/редактирования партнера
/// </summary>
public partial class PartnerEditWindow : Window
{
    private readonly PartnerViewModel? _existingPartner;
    private readonly List<PartnerType> _partnerTypes;
    private readonly IPartnerService _partnerService;

    public PartnerEditWindow(PartnerViewModel? partner, List<PartnerType> partnerTypes, IPartnerService partnerService)
    {
        InitializeComponent();
        _existingPartner = partner;
        _partnerTypes = partnerTypes;
        _partnerService = partnerService;

        Title = partner == null ? "Добавление партнера" : "Редактирование партнера";
        TitleText.Text = partner == null ? "Добавление партнера" : "Редактирование партнера";

        InitializeForm();
    }

    private void InitializeForm()
    {
        // Заполнение выпадающего списка типов партнеров
        PartnerTypeComboBox.ItemsSource = _partnerTypes;

        if (_existingPartner != null)
        {
            // Заполнение полей данными существующего партнера
            NameTextBox.Text = _existingPartner.Name;
            PartnerTypeComboBox.SelectedValue = _existingPartner.PartnerTypeId;
            RatingTextBox.Text = _existingPartner.Rating.ToString();
            LegalAddressTextBox.Text = _existingPartner.LegalAddress;
            InnTextBox.Text = _existingPartner.Inn;
            DirectorNameTextBox.Text = _existingPartner.DirectorName;
            PhoneTextBox.Text = _existingPartner.Phone;
            EmailTextBox.Text = _existingPartner.Email;
        }
        else
        {
            // Выбор первого типа партнера по умолчанию
            if (_partnerTypes.Count > 0)
                PartnerTypeComboBox.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Валидация данных формы
    /// </summary>
    private bool ValidateForm(out string errorMessage)
    {
        errorMessage = string.Empty;

        // Проверка наименования
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            errorMessage = "Наименование компании является обязательным полем.";
            NameTextBox.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            return false;
        }
        NameTextBox.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 209, 209, 209));

        // Проверка типа партнера
        if (PartnerTypeComboBox.SelectedItem == null)
        {
            errorMessage = "Необходимо выбрать тип партнера.";
            return false;
        }

        // Проверка рейтинга
        if (!int.TryParse(RatingTextBox.Text, out int rating) || rating < 0)
        {
            RatingErrorText.Text = "Рейтинг должен быть неотрицательным целым числом.";
            RatingErrorText.Visibility = Visibility.Visible;
            RatingTextBox.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            errorMessage = "Рейтинг должен быть неотрицательным целым числом.";
            return false;
        }
        RatingErrorText.Visibility = Visibility.Collapsed;
        RatingTextBox.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 209, 209, 209));

        // Проверка email
        if (!string.IsNullOrWhiteSpace(EmailTextBox.Text))
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(EmailTextBox.Text, emailPattern))
            {
                EmailErrorText.Text = "Введите корректный адрес электронной почты.";
                EmailErrorText.Visibility = Visibility.Visible;
                errorMessage = "Некорректный формат email.";
                return false;
            }
        }
        EmailErrorText.Visibility = Visibility.Collapsed;

        return true;
    }

    /// <summary>
    /// Сохранение данных партнера
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
            if (_existingPartner != null)
            {
                // Обновление существующего партнера
                _existingPartner.Name = NameTextBox.Text.Trim();
                _existingPartner.PartnerTypeId = (int)PartnerTypeComboBox.SelectedValue!;
                _existingPartner.PartnerTypeName = ((PartnerType)PartnerTypeComboBox.SelectedItem!).Name;
                _existingPartner.Rating = int.Parse(RatingTextBox.Text);
                _existingPartner.LegalAddress = LegalAddressTextBox.Text.Trim();
                _existingPartner.Inn = InnTextBox.Text.Trim();
                _existingPartner.DirectorName = DirectorNameTextBox.Text.Trim();
                _existingPartner.Phone = PhoneTextBox.Text.Trim();
                _existingPartner.Email = EmailTextBox.Text.Trim();

                await _partnerService.UpdatePartnerAsync(_existingPartner);
            }
            else
            {
                // Добавление нового партнера
                var newPartner = new PartnerViewModel
                {
                    Name = NameTextBox.Text.Trim(),
                    PartnerTypeId = (int)PartnerTypeComboBox.SelectedValue!,
                    PartnerTypeName = ((PartnerType)PartnerTypeComboBox.SelectedItem!).Name,
                    Rating = int.Parse(RatingTextBox.Text),
                    LegalAddress = LegalAddressTextBox.Text.Trim(),
                    Inn = InnTextBox.Text.Trim(),
                    DirectorName = DirectorNameTextBox.Text.Trim(),
                    Phone = PhoneTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim()
                };

                await _partnerService.AddPartnerAsync(newPartner);
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Произошла ошибка при сохранении данных.\n\nОшибка: {ex.Message}",
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
