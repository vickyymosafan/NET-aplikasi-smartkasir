using System.Windows.Threading;

namespace SmartKasir.Client.Services;

/// <summary>
/// Implementation dari ISessionService untuk tracking aktivitas dan auto-logout
/// </summary>
public class SessionService : ISessionService, IDisposable
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly DispatcherTimer _inactivityTimer;
    private readonly DispatcherTimer _countdownTimer;
    
    private DateTime _lastActivityTime;
    private bool _isTracking;
    private bool _disposed;

    /// <summary>
    /// Timeout dalam menit (30 menit sesuai Requirements 14.5)
    /// </summary>
    public const int DefaultTimeoutMinutes = 30;
    
    /// <summary>
    /// Warning threshold dalam menit (5 menit sebelum timeout)
    /// </summary>
    public const int WarningThresholdMinutes = 5;

    public event EventHandler<SessionExpiredEventArgs>? SessionExpired;
    public event EventHandler<SessionWarningEventArgs>? SessionWarning;

    public int TimeoutMinutes { get; }
    
    public int RemainingSeconds
    {
        get
        {
            if (!_isTracking) return 0;
            var elapsed = DateTime.UtcNow - _lastActivityTime;
            var remaining = TimeSpan.FromMinutes(TimeoutMinutes) - elapsed;
            return Math.Max(0, (int)remaining.TotalSeconds);
        }
    }

    public bool IsTracking => _isTracking;

    public SessionService(
        IAuthService authService, 
        INavigationService navigationService,
        int? timeoutMinutes = null)
    {
        _authService = authService;
        _navigationService = navigationService;
        TimeoutMinutes = timeoutMinutes ?? DefaultTimeoutMinutes;

        // Timer untuk check inaktivitas setiap menit
        _inactivityTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _inactivityTimer.Tick += OnInactivityTimerTick;

        // Timer untuk countdown warning setiap detik
        _countdownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _countdownTimer.Tick += OnCountdownTimerTick;

        _lastActivityTime = DateTime.UtcNow;
    }

    public void StartTracking()
    {
        if (_disposed) return;
        if (!_authService.IsAuthenticated) return;

        _lastActivityTime = DateTime.UtcNow;
        _isTracking = true;
        _inactivityTimer.Start();
    }

    public void StopTracking()
    {
        _isTracking = false;
        _inactivityTimer.Stop();
        _countdownTimer.Stop();
    }

    public void ResetTimer()
    {
        if (!_isTracking) return;
        
        _lastActivityTime = DateTime.UtcNow;
        _countdownTimer.Stop(); // Stop warning countdown jika ada
    }

    private void OnInactivityTimerTick(object? sender, EventArgs e)
    {
        if (!_isTracking || !_authService.IsAuthenticated)
        {
            StopTracking();
            return;
        }

        var elapsed = DateTime.UtcNow - _lastActivityTime;
        var timeoutSpan = TimeSpan.FromMinutes(TimeoutMinutes);
        var warningSpan = TimeSpan.FromMinutes(TimeoutMinutes - WarningThresholdMinutes);

        // Check jika sudah timeout
        if (elapsed >= timeoutSpan)
        {
            HandleSessionExpired();
            return;
        }

        // Check jika masuk warning zone (5 menit sebelum timeout)
        if (elapsed >= warningSpan && !_countdownTimer.IsEnabled)
        {
            _countdownTimer.Start();
        }
    }

    private void OnCountdownTimerTick(object? sender, EventArgs e)
    {
        var remaining = RemainingSeconds;
        
        if (remaining <= 0)
        {
            HandleSessionExpired();
            return;
        }

        // Fire warning event setiap detik selama countdown
        SessionWarning?.Invoke(this, new SessionWarningEventArgs
        {
            RemainingSeconds = remaining
        });
    }

    private async void HandleSessionExpired()
    {
        StopTracking();

        SessionExpired?.Invoke(this, new SessionExpiredEventArgs
        {
            ExpiredAt = DateTime.UtcNow,
            Reason = $"Sesi berakhir karena tidak ada aktivitas selama {TimeoutMinutes} menit"
        });

        // Logout dan navigate ke login
        await _authService.LogoutAsync();
        _navigationService.NavigateTo("Login");
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        StopTracking();
        _inactivityTimer.Tick -= OnInactivityTimerTick;
        _countdownTimer.Tick -= OnCountdownTimerTick;
    }
}
