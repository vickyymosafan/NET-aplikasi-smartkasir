namespace SmartKasir.Client.Services;

/// <summary>
/// Service untuk sinkronisasi data antara local dan server
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Sync data ke server
    /// </summary>
    Task SyncToServerAsync();

    /// <summary>
    /// Sync data dari server
    /// </summary>
    Task SyncFromServerAsync();

    /// <summary>
    /// Check apakah koneksi ke server tersedia
    /// </summary>
    bool IsOnline { get; }

    /// <summary>
    /// Get status sinkronisasi terakhir
    /// </summary>
    DateTime? LastSyncTime { get; }

    /// <summary>
    /// Get jumlah pending transactions
    /// </summary>
    int PendingTransactionCount { get; }

    /// <summary>
    /// Event ketika sync status berubah
    /// </summary>
    event EventHandler<SyncStatusEventArgs>? SyncStatusChanged;

    /// <summary>
    /// Event ketika connection status berubah
    /// </summary>
    event EventHandler<ConnectionStatusEventArgs>? ConnectionStatusChanged;
}

/// <summary>
/// Event args untuk sync status change
/// </summary>
public class SyncStatusEventArgs : EventArgs
{
    public SyncStatusType Status { get; set; }
    public string? Message { get; set; }
    public int? ProgressPercentage { get; set; }
    public Exception? Error { get; set; }
}

/// <summary>
/// Event args untuk connection status change
/// </summary>
public class ConnectionStatusEventArgs : EventArgs
{
    public bool IsOnline { get; set; }
    public DateTime ChangedAt { get; set; }
}

/// <summary>
/// Tipe status sinkronisasi
/// </summary>
public enum SyncStatusType
{
    Idle,
    Syncing,
    Completed,
    Failed,
    Conflict
}
