using System.Windows;
using System.Windows.Input;
using SmartKasir.Client.Services;

namespace SmartKasir.Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ISessionService? _sessionService;
    private readonly IAuthService? _authService;
    private readonly INavigationService? _navigationService;

    // Default constructor required for XAML designer
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(
        ISessionService sessionService,
        IAuthService authService,
        INavigationService navigationService)
    {
        InitializeComponent();
        
        _sessionService = sessionService;
        _authService = authService;
        _navigationService = navigationService;

        // Set the content control for navigation
        if (_navigationService != null)
        {
            _navigationService.SetMainContent(MainContent);
        }

        // Subscribe to auth status changes
        _authService.AuthStatusChanged += OnAuthStatusChanged;
        
        // Subscribe to session events
        _sessionService.SessionExpired += OnSessionExpired;
        _sessionService.SessionWarning += OnSessionWarning;

        // Hook global input events for activity tracking
        PreviewMouseMove += OnUserActivity;
        PreviewMouseDown += OnUserActivity;
        PreviewKeyDown += OnUserActivity;
        PreviewTouchDown += OnUserActivity;

        Loaded += OnWindowLoaded;
        Closing += OnWindowClosing;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        // Debug logging
        Console.WriteLine($"[MainWindow] OnWindowLoaded called");
        Console.WriteLine($"[MainWindow] LoginOverlay Visibility: {LoginOverlay?.Visibility}");
        Console.WriteLine($"[MainWindow] UsernameTextBox exists: {UsernameTextBox != null}");
        Console.WriteLine($"[MainWindow] LoginButton exists: {LoginButton != null}");
        
        // Focus username textbox
        UsernameTextBox?.Focus();
        
        // Start session tracking if already authenticated
        if (_authService?.IsAuthenticated == true)
        {
            Console.WriteLine($"[MainWindow] User already authenticated, showing dashboard");
            _sessionService?.StartTracking();
            ShowDashboard();
        }
        else
        {
            Console.WriteLine($"[MainWindow] User not authenticated, showing login form");
        }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        if (_authService == null) return;

        var username = UsernameTextBox?.Text?.Trim();
        var password = PasswordBox?.Password;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Username dan password harus diisi");
            return;
        }

        try
        {
            LoginButton.IsEnabled = false;
            LoadingIndicator.Visibility = Visibility.Visible;
            ErrorBorder.Visibility = Visibility.Collapsed;

            var result = await _authService.LoginAsync(username, password);
            
            if (result.Success)
            {
                ShowDashboard();
            }
            else
            {
                ShowError(result.ErrorMessage ?? "Login gagal");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error: {ex.Message}");
        }
        finally
        {
            LoginButton.IsEnabled = true;
            LoadingIndicator.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowError(string message)
    {
        ErrorMessageBlock.Text = message;
        ErrorBorder.Visibility = Visibility.Visible;
    }

    private void ShowDashboard()
    {
        Console.WriteLine($"[MainWindow] ShowDashboard called");
        Console.WriteLine($"[MainWindow] MainContent exists: {MainContent != null}");
        Console.WriteLine($"[MainWindow] NavigationService exists: {_navigationService != null}");
        
        LoginOverlay.Visibility = Visibility.Collapsed;
        Console.WriteLine($"[MainWindow] LoginOverlay hidden");
        
        try
        {
            Console.WriteLine($"[MainWindow] Calling NavigateTo<DashboardView>");
            _navigationService?.NavigateTo<SmartKasir.Client.Views.DashboardView>();
            Console.WriteLine($"[MainWindow] NavigateTo completed");
            Console.WriteLine($"[MainWindow] MainContent.Content type: {MainContent?.Content?.GetType().Name}");
            Console.WriteLine($"[MainWindow] MainContent.Content is null: {MainContent?.Content == null}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MainWindow] ERROR in ShowDashboard: {ex.Message}");
            Console.WriteLine($"[MainWindow] Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[MainWindow] Inner exception: {ex.InnerException.Message}");
            }
        }
    }

    private void ShowLogin()
    {
        LoginOverlay.Visibility = Visibility.Visible;
        MainContent.Content = null;
        UsernameTextBox.Text = "";
        PasswordBox.Password = "";
        ErrorBorder.Visibility = Visibility.Collapsed;
        UsernameTextBox?.Focus();
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Cleanup
        _sessionService?.StopTracking();
        if (_authService != null)
            _authService.AuthStatusChanged -= OnAuthStatusChanged;
        if (_sessionService != null)
        {
            _sessionService.SessionExpired -= OnSessionExpired;
            _sessionService.SessionWarning -= OnSessionWarning;
        }

        if (_sessionService is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private void OnAuthStatusChanged(object? sender, AuthStatusChangedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (e.IsAuthenticated)
            {
                _sessionService?.StartTracking();
                ShowDashboard();
            }
            else
            {
                _sessionService?.StopTracking();
                ShowLogin();
            }
        });
    }

    private void OnUserActivity(object? sender, InputEventArgs e)
    {
        // Reset session timer on any user activity
        _sessionService?.ResetTimer();
    }

    private void OnSessionExpired(object? sender, SessionExpiredEventArgs e)
    {
        // Show notification to user
        Dispatcher.Invoke(() =>
        {
            MessageBox.Show(
                this,
                e.Reason,
                "Sesi Berakhir",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        });
    }

    private void OnSessionWarning(object? sender, SessionWarningEventArgs e)
    {
        // Optional: Show warning in status bar or notification
        // Only show message box at specific intervals to avoid spam
        if (e.RemainingSeconds == 300 || // 5 minutes
            e.RemainingSeconds == 60 ||  // 1 minute
            e.RemainingSeconds == 30)    // 30 seconds
        {
            Dispatcher.Invoke(() =>
            {
                var minutes = e.RemainingSeconds / 60;
                var seconds = e.RemainingSeconds % 60;
                var timeStr = minutes > 0 
                    ? $"{minutes} menit {seconds} detik" 
                    : $"{seconds} detik";

                // Use non-blocking notification (could be replaced with toast/snackbar)
                System.Diagnostics.Debug.WriteLine(
                    $"Session Warning: {timeStr} tersisa sebelum auto-logout");
            });
        }
    }
}
