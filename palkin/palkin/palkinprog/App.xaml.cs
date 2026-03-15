using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PalkinLib.Data;
using PalkinLib.Repositories;
using PalkinLib.Services;
using palkinprog.Dialogs;

namespace palkinprog;

/// <summary>
/// Главный класс приложения с настройкой зависимостей
/// </summary>
public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public App()
    {
        Startup += AppStartup;
    }

    private void AppStartup(object sender, StartupEventArgs e)
    {
        _serviceProvider = ConfigureServices();
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }

    /// <summary>
    /// Получение сервиса из контейнера
    /// </summary>
    public T? GetService<T>() where T : class
    {
        return _serviceProvider?.GetService<T>();
    }

    /// <summary>
    /// Настройка внедрения зависимостей
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Строка подключения к PostgreSQL
        // Для продакшена используйте безопасное хранение строк подключения
        var connectionString = "Host=localhost;Port=5432;Database=Palkin;Username=app;Password=123456789";

        // Регистрация DbContext как фабрики
        services.AddTransient<Func<PalkinDbContext>>(provider => () => new PalkinDbContext(connectionString));

        // Регистрация репозиториев
        services.AddTransient<IPartnerTypeRepository, PartnerTypeRepository>();
        services.AddTransient<IPartnerRepository, PartnerRepository>();
        services.AddTransient<ISaleRepository, SaleRepository>();

        // Регистрация сервисов
        services.AddTransient<IPartnerService, PartnerService>();

        return services.BuildServiceProvider();
    }
}
