using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartKasir.Client.Services;

namespace SmartKasir.Client.ViewModels;

/// <summary>
/// ViewModel untuk BackupView - Backup dan Restore database (Admin only)
/// Requirements: 10.1, 10.2, 10.3, 10.4, 10.5
/// </summary>
public partial class BackupViewModel : ObservableObject
{
    private readonly IBackupService _backupService;

    [ObservableProperty]
    private ObservableCollection<BackupFileInfo> backups = new();

    [ObservableProperty]
    private BackupFileInfo? selectedBackup;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isProcessing;

    [ObservableProperty]
    private int progressPercent;

    [ObservableProperty]
    private string? progressMessage;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? successMessage;

    [ObservableProperty]
    private bool showRestoreConfirmation;

    [ObservableProperty]
    private string backupDirectory = string.Empty;

    public BackupViewModel(IBackupService backupService)
    {
        _backupService = backupService;
        _backupService.ProgressChanged += OnProgressChanged;
        BackupDirectory = backupService.BackupDirectory;
    }

    /// <summary>
    /// Load daftar backup yang tersedia
    /// </summary>
    [RelayCommand]
    public async Task LoadBackupsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var backupList = await _backupService.GetAvailableBackupsAsync();
            Backups = new ObservableCollection<BackupFileInfo>(backupList);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal memuat daftar backup: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Create backup baru (Requirement 10.1)
    /// </summary>
    [RelayCommand]
    public async Task CreateBackupAsync()
    {
        try
        {
            IsProcessing = true;
            ErrorMessage = null;
            SuccessMessage = null;
            ProgressPercent = 0;

            var result = await _backupService.CreateBackupAsync();

            if (result.Success)
            {
                SuccessMessage = $"Backup berhasil dibuat: {result.FileName} ({FormatFileSize(result.FileSizeBytes)})";
                await LoadBackupsAsync();
            }
            else
            {
                // Requirement 10.5 - Show error with detail
                ErrorMessage = $"Backup gagal: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Backup gagal: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    /// Show restore confirmation dialog (Requirement 10.3)
    /// </summary>
    [RelayCommand]
    public void RequestRestore()
    {
        if (SelectedBackup == null) return;
        ShowRestoreConfirmation = true;
    }

    /// <summary>
    /// Cancel restore
    /// </summary>
    [RelayCommand]
    public void CancelRestore()
    {
        ShowRestoreConfirmation = false;
    }

    /// <summary>
    /// Confirm and execute restore (Requirement 10.4)
    /// </summary>
    [RelayCommand]
    public async Task ConfirmRestoreAsync()
    {
        if (SelectedBackup == null) return;

        try
        {
            ShowRestoreConfirmation = false;
            IsProcessing = true;
            ErrorMessage = null;
            SuccessMessage = null;
            ProgressPercent = 0;

            var result = await _backupService.RestoreBackupAsync(SelectedBackup.FilePath);

            if (result.Success)
            {
                SuccessMessage = $"Database berhasil di-restore dari {SelectedBackup.FileName}";
            }
            else
            {
                // Requirement 10.5 - Show error with detail
                ErrorMessage = $"Restore gagal: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Restore gagal: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    /// Delete backup file
    /// </summary>
    [RelayCommand]
    public async Task DeleteBackupAsync()
    {
        if (SelectedBackup == null) return;

        try
        {
            var success = await _backupService.DeleteBackupAsync(SelectedBackup.FilePath);
            
            if (success)
            {
                SuccessMessage = $"Backup {SelectedBackup.FileName} berhasil dihapus";
                await LoadBackupsAsync();
            }
            else
            {
                ErrorMessage = "Gagal menghapus file backup";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal menghapus backup: {ex.Message}";
        }
    }

    /// <summary>
    /// Open backup directory in explorer
    /// </summary>
    [RelayCommand]
    public void OpenBackupDirectory()
    {
        try
        {
            System.Diagnostics.Process.Start("explorer.exe", BackupDirectory);
        }
        catch
        {
            // Ignore if can't open
        }
    }

    private void OnProgressChanged(object? sender, BackupProgressEventArgs e)
    {
        ProgressPercent = e.ProgressPercent;
        ProgressMessage = e.Message;
    }

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
