namespace SmartKasir.Client.Services;

/// <summary>
/// Service untuk manajemen sesi pengguna dengan auto-logout pada inaktivitas
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Mulai tracking aktivitas pengguna
    /// </summary>
    void StartTracking();

    /// <summary>
    /// Hentikan tracking aktivitas
    /// </summary>
    void StopTracking();

    /// <summary>
    /// Reset timer inaktivitas (dipanggil saat ada aktivitas)
    /// </summary>
    void ResetTimer();

    /// <summary>
    /// Durasi timeout dalam menit (default: 30)
    /// </summary>
    int TimeoutMinutes { get; }

    /// <summary>
    /// Waktu tersisa sebelum timeout dalam detik
    /// </summary>
    int RemainingSeconds { get; }

    /// <summary>
    /// Status apakah tracking sedang aktif
    /// </summary>
    bool IsTracking { get; }

    /// <summary>
    /// Event ketika sesi expired karena inaktivitas
    /// </summary>
    event EventHandler<SessionExpiredEventArgs>? SessionExpired;

    /// <summary>
    /// Event ketika waktu tersisa berubah (untuk UI warning)
    /// </summary>
    event EventHandler<SessionWarningEventArgs>? SessionWarning;
}

/// <summary>
/// Event args untuk session expired
/// </summary>
public class SessionExpiredEventArgs : EventArgs
{
    public DateTime ExpiredAt { get; set; }
    public string Reason { get; set; } = "Inactivity timeout";
}

/// <summary>
/// Event args untuk session warning (5 menit sebelum timeout)
/// </summary>
public class SessionWarningEventArgs : EventArgs
{
    public int RemainingSeconds { get; set; }
}
