using System.IO;
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Refit;
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

    private static void Log(string message)
    {
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup.log");
        var logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
        File.AppendAllText(logPath, logMessage + "\n");
        Console.WriteLine(logMessage);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Allocate console for debugging
        AllocConsole();
        
        Console.WriteLine("========================================");
        Console.WriteLine("=== SmartKasir Client Starting ===");
        Console.WriteLine("========================================");
        
        Log("=== SmartKasir Client Starting ===");
        Log($"Current Directory: {Environment.CurrentDirectory}");
        Log($"Base Directory: {AppDomain.CurrentDomain.BaseDirectory}");
        
        // Global exception handlers
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = (Exception)args.ExceptionObject;
            Log($"[FATAL] Unhandled Exception: {ex.Message}");
            Log($"[FATAL] Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Log($"[FATAL] Inner Exception: {ex.InnerException.Message}");
                Log($"[FATAL] Inner Stack: {ex.InnerException.StackTrace}");
            }
            Console.WriteLine("\n\nPress any key to exit...");
            Console.ReadKey();
        };
        
        DispatcherUnhandledException += (s, args) =>
        {
            Log($"[ERROR] UI Exception: {args.Exception.Message}");
            Log($"[ERROR] Stack Trace: {args.Exception.StackTrace}");
            if (args.Exception.InnerException != null)
            {
                Log($"[ERROR] Inner Exception: {args.Exception.InnerException.Message}");
            }
            args.Handled = true;
        };
        
        try
        {
            Log("Step 1: Configuring services...");
            var services = new ServiceCollection();
            ConfigureServices(services);
            
            Log("Step 2: Building service provider...");
            _serviceProvider = services.BuildServiceProvider();
            Log("Service provider built successfully!");
            
            Log("Step 3: Initializing database...");
            InitializeDatabase();
            Log("Database initialized!");
            
            Log("Step 4: Resolving MainWindow from DI...");
            var mainWindow = ResolveMainWindow();
            
            if (mainWindow == null)
            {
                Log("[FATAL] MainWindow is null!");
                Console.WriteLine("\n\nPress any key to exit...");
                Console.ReadKey();
                Shutdown(1);
                return;
            }
            
            Log($"MainWindow created: {mainWindow != null}");
            Log($"MainWindow Title: {mainWindow.Title}");
            Log($"MainWindow Size: {mainWindow.Width}x{mainWindow.Height}");
            
            Log("Step 5: Showing MainWindow...");
            
            // Ensure window is on screen and visible
            mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Topmost = true; // Temporarily bring to front
            
            mainWindow.Show();
            
            // Activate and focus
            mainWindow.Activate();
            mainWindow.Focus();
            
            // Reset topmost after showing
            mainWindow.Topmost = false;
            
            Log($"MainWindow IsVisible: {mainWindow.IsVisible}");
            Log($"MainWindow WindowState: {mainWindow.WindowState}");
            Log($"MainWindow Left: {mainWindow.Left}, Top: {mainWindow.Top}");
            Log($"MainWindow ActualWidth: {mainWindow.ActualWidth}, ActualHeight: {mainWindow.ActualHeight}");
            Log("========================================");
            Log("Application started successfully!");
            Log("========================================");
            
            Console.WriteLine("\n=== Application Running ===");
            Console.WriteLine("Window should now be visible on screen.");
            Console.WriteLine($"Position: Left={mainWindow.Left}, Top={mainWindow.Top}");
            Console.WriteLine($"Size: {mainWindow.ActualWidth}x{mainWindow.ActualHeight}");
            Console.WriteLine("\nIf still not visible, the window might be rendering but with issues.");
            Console.WriteLine("Press Ctrl+C to close the application.");
        }
        catch (Exception ex)
        {
            Log($"[FATAL] Startup Error: {ex.Message}");
            Log($"[FATAL] Exception Type: {ex.GetType().FullName}");
            Log($"[FATAL] Stack Trace: {ex.StackTrace}");
            
            var innerEx = ex.InnerException;
            int depth = 1;
            while (innerEx != null)
            {
                Log($"[FATAL] Inner Exception {depth}: {innerEx.Message}");
                Log($"[FATAL] Inner Type {depth}: {innerEx.GetType().FullName}");
                Log($"[FATAL] Inner Stack {depth}: {innerEx.StackTrace}");
                innerEx = innerEx.InnerException;
                depth++;
            }
            
            Console.WriteLine("\n\n=== STARTUP FAILED ===");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
            Shutdown(1);
        }
    }

    private MainWindow? ResolveMainWindow()
    {
        try
        {
            Log("  - Resolving INavigationService...");
            var navService = _serviceProvider!.GetRequiredService<INavigationService>();
            Log($"    INavigationService resolved: {navService != null}");
            
            Log("  - Resolving IAuthService...");
            var authService = _serviceProvider!.GetRequiredService<IAuthService>();
            Log($"    IAuthService resolved: {authService != null}");
            
            Log("  - Resolving ISessionService...");
            var sessionService = _serviceProvider!.GetRequiredService<ISessionService>();
            Log($"    ISessionService resolved: {sessionService != null}");
            
            Log("  - Resolving MainWindow...");
            var mainWindow = _serviceProvider!.GetRequiredService<MainWindow>();
            Log($"    MainWindow resolved: {mainWindow != null}");
            
            return mainWindow;
        }
        catch (Exception ex)
        {
            Log($"[ERROR] Failed to resolve service: {ex.Message}");
            Log($"[ERROR] Exception Type: {ex.GetType().FullName}");
            
            var innerEx = ex.InnerException;
            int depth = 1;
            while (innerEx != null)
            {
                Log($"[ERROR] Inner {depth}: {innerEx.Message}");
                innerEx = innerEx.InnerException;
                depth++;
            }
            
            throw;
        }
    }

    private void InitializeDatabase()
    {
        try
        {
            using var context = new LocalDbContext();
            context.Database.EnsureCreated();
            Log("  - Database file created/verified");
        }
        catch (Exception ex)
        {
            Log($"[WARN] Database init error (non-fatal): {ex.Message}");
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        Log("  - Registering DbContext...");
        services.AddDbContext<LocalDbContext>();
        
        Log("  - Registering Refit API Client...");
        // IMPORTANT: Use AddRefitClient for Refit interfaces!
        services.AddRefitClient<ISmartKasirApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5146");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
        
        Log("  - Registering Services...");
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<ISyncService, SyncService>();
        services.AddSingleton<IProductService, ProductService>();
        services.AddSingleton<ITransactionService, TransactionService>();
        services.AddSingleton<IPrinterService, PrinterService>();
        services.AddSingleton<IBackupService, BackupService>();
        
        Log("  - Registering ViewModels...");
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProductGridViewModel>();
        services.AddTransient<CartViewModel>();
        services.AddTransient<CheckoutViewModel>();
        services.AddTransient<ProductManagementViewModel>();
        services.AddTransient<CategoryManagementViewModel>();
        services.AddTransient<UserManagementViewModel>();
        services.AddTransient<ReportViewModel>();
        services.AddTransient<BackupViewModel>();

        Log("  - Registering Views...");
        services.AddTransient<LoginView>();
        services.AddTransient<DashboardView>();
        services.AddTransient<ProductGridView>();
        services.AddTransient<CartView>();
        services.AddTransient<CheckoutDialog>();
        services.AddTransient<ProductManagementView>();
        services.AddTransient<CategoryManagementView>();
        services.AddTransient<UserManagementView>();
        services.AddTransient<ReportView>();
        services.AddTransient<BackupView>();

        Log("  - Registering MainWindow...");
        services.AddSingleton<MainWindow>();
        
        Log("  - All services registered!");
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    protected override void OnExit(ExitEventArgs e)
    {
        Log("Application exiting...");
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
