using System.Windows;
using System.Windows.Input;
using SmartKasir.Client.Services;

namespace SmartKasir;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ISessionService _sessionService;
    private readonly IAuthService _authService;

    public MainWindow(
        ISessionService sessionService,
        IAuthService authService)
    {
        InitializeComponent();
        
        _sessionService = sessionService;
        _authService = authService;

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
        // Start session tracking if already authenticated
        if (_authService.IsAuthenticated)
        {
            _sessionService.StartTracking();
        }
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Cleanup
        _sessionService.StopTracking();
        _authService.AuthStatusChanged -= OnAuthStatusChanged;
        _sessionService.SessionExpired -= OnSessionExpired;
        _sessionService.SessionWarning -= OnSessionWarning;

        if (_sessionService is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private void OnAuthStatusChanged(object? sender, AuthStatusChangedEventArgs e)
    {
        if (e.IsAuthenticated)
        {
            _sessionService.StartTracking();
        }
        else
        {
            _sessionService.StopTracking();
        }
    }

    private void OnUserActivity(object? sender, InputEventArgs e)
    {
        // Reset session timer on any user activity
        _sessionService.ResetTimer();
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
