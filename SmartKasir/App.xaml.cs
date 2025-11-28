using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SmartKasir.Client.Services;
using SmartKasir.Client.ViewModels;
using SmartKasir.Client.Views;
using SmartKasir.Shared.Data;

namespace SmartKasir.Client;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register DbContext
        services.AddScoped<LocalDbContext>();
        
        // Register Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ISyncService, SyncService>();
        services.AddScoped<IPrinterService, PrinterService>();
        services.AddScoped<INavigationService, NavigationService>();
        
        // Register API Client
        services.AddHttpClient<ISmartKasirApi>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:5001");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(GetRetryPolicy());
        
        // Register ViewModels
        services.AddScoped<LoginViewModel>();
        services.AddScoped<DashboardViewModel>();
        services.AddScoped<ProductGridViewModel>();
        services.AddScoped<CartViewModel>();
        services.AddScoped<CheckoutViewModel>();

        // Register Views
        services.AddScoped<LoginView>();
        services.AddScoped<DashboardView>();
        services.AddScoped<ProductGridView>();
        services.AddScoped<CartView>();
        services.AddScoped<CheckoutDialog>();

        // Register MainWindow
        services.AddSingleton<MainWindow>();
    }

    private static Polly.IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return Polly.HttpClientFactory.HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    System.Diagnostics.Debug.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds}s");
                });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

