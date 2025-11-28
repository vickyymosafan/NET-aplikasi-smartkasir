namespace SmartKasir.Client.Services;

/// <summary>
/// Service untuk backup dan restore database PostgreSQL
/// Requirements: 10.1, 10.2, 10.3, 10.4, 10.5
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Create backup database (Requirement 10.1)
    /// </summary>
    Task<BackupResult> CreateBackupAsync(string? outputPath = null);

    /// <summary>
    /// Restore database dari file backup (Requirement 10.4)
    /// </summary>
    Task<RestoreResult> RestoreBackupAsync(string backupFilePath);

    /// <summary>
    /// Get daftar file backup yang tersedia
    /// </summary>
    Task<IEnumerable<BackupFileInfo>> GetAvailableBackupsAsync();

    /// <summary>
    /// Delete file backup
    /// </summary>
    Task<bool> DeleteBackupAsync(string backupFilePath);

    /// <summary>
    /// Get default backup directory
    /// </summary>
    string BackupDirectory { get; }

    /// <summary>
    /// Event ketika backup/restore progress berubah
    /// </summary>
    event EventHandler<BackupProgressEventArgs>? ProgressChanged;
}

/// <summary>
/// Result dari operasi backup
/// </summary>
public class BackupResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result dari operasi restore
/// </summary>
public class RestoreResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime RestoredAt { get; set; }
}

/// <summary>
/// Info file backup
/// </summary>
public class BackupFileInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }

    public string FileSizeFormatted => FormatFileSize(FileSizeBytes);

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}

/// <summary>
/// Event args untuk backup/restore progress
/// </summary>
public class BackupProgressEventArgs : EventArgs
{
    public string Operation { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
    public string? Message { get; set; }
    public bool IsComplete { get; set; }
    public bool HasError { get; set; }
}
