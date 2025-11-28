using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace SmartKasir.Client.Services;

/// <summary>
/// Implementation dari IBackupService
/// Menggunakan pg_dump dan pg_restore untuk PostgreSQL
/// Requirements: 10.1, 10.2, 10.3, 10.4, 10.5
/// </summary>
public class BackupService : IBackupService
{
    private readonly string _host;
    private readonly string _database;
    private readonly string _username;
    private readonly string _password;
    private readonly string _backupDirectory;

    public event EventHandler<BackupProgressEventArgs>? ProgressChanged;

    public string BackupDirectory => _backupDirectory;

    public BackupService(IConfiguration configuration)
    {
        // Parse connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=smartkasir;Username=postgres;Password=Admin";

        var parts = connectionString.Split(';')
            .Select(p => p.Split('='))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0].Trim(), p => p[1].Trim(), StringComparer.OrdinalIgnoreCase);

        _host = parts.GetValueOrDefault("Host", "localhost");
        _database = parts.GetValueOrDefault("Database", "smartkasir");
        _username = parts.GetValueOrDefault("Username", "postgres");
        _password = parts.GetValueOrDefault("Password", "");

        // Set backup directory
        _backupDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SmartKasir",
            "Backups");

        // Ensure directory exists
        Directory.CreateDirectory(_backupDirectory);
    }

    /// <summary>
    /// Create backup using pg_dump (Requirement 10.1, 10.2)
    /// </summary>
    public async Task<BackupResult> CreateBackupAsync(string? outputPath = null)
    {
        var result = new BackupResult { CreatedAt = DateTime.Now };

        try
        {
            OnProgressChanged("Backup", 0, "Memulai backup database...");

            // Generate filename with timestamp (Requirement 10.2)
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"smartkasir_backup_{timestamp}.sql";
            var filePath = outputPath ?? Path.Combine(_backupDirectory, fileName);

            OnProgressChanged("Backup", 20, "Menjalankan pg_dump...");

            // Build pg_dump command
            var pgDumpPath = FindPgDump();
            if (string.IsNullOrEmpty(pgDumpPath))
            {
                throw new InvalidOperationException(
                    "pg_dump tidak ditemukan. Pastikan PostgreSQL terinstall dan PATH dikonfigurasi.");
            }


            var processInfo = new ProcessStartInfo
            {
                FileName = pgDumpPath,
                Arguments = $"-h {_host} -U {_username} -d {_database} -F p -f \"{filePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Environment = { ["PGPASSWORD"] = _password }
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();

            OnProgressChanged("Backup", 50, "Menyimpan data...");

            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"pg_dump gagal: {error}");
            }

            OnProgressChanged("Backup", 90, "Memverifikasi file backup...");

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists || fileInfo.Length == 0)
            {
                throw new InvalidOperationException("File backup tidak valid atau kosong.");
            }

            result.Success = true;
            result.FilePath = filePath;
            result.FileName = fileName;
            result.FileSizeBytes = fileInfo.Length;

            OnProgressChanged("Backup", 100, $"Backup berhasil: {fileName}", true);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            OnProgressChanged("Backup", 0, $"Backup gagal: {ex.Message}", true, true);
        }

        return result;
    }

    /// <summary>
    /// Restore database using pg_restore (Requirement 10.4)
    /// </summary>
    public async Task<RestoreResult> RestoreBackupAsync(string backupFilePath)
    {
        var result = new RestoreResult { RestoredAt = DateTime.Now };

        try
        {
            if (!File.Exists(backupFilePath))
            {
                throw new FileNotFoundException($"File backup tidak ditemukan: {backupFilePath}");
            }

            OnProgressChanged("Restore", 0, "Memulai restore database...");

            var psqlPath = FindPsql();
            if (string.IsNullOrEmpty(psqlPath))
            {
                throw new InvalidOperationException(
                    "psql tidak ditemukan. Pastikan PostgreSQL terinstall dan PATH dikonfigurasi.");
            }

            OnProgressChanged("Restore", 20, "Menghapus data lama...");

            // Drop and recreate database connections
            var dropConnections = $"-h {_host} -U {_username} -d postgres -c \"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{_database}' AND pid <> pg_backend_pid();\"";
            await RunPsqlCommandAsync(psqlPath, dropConnections);

            OnProgressChanged("Restore", 40, "Menjalankan restore...");

            // Run restore using psql (for plain SQL format)
            var processInfo = new ProcessStartInfo
            {
                FileName = psqlPath,
                Arguments = $"-h {_host} -U {_username} -d {_database} -f \"{backupFilePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Environment = { ["PGPASSWORD"] = _password }
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();

            OnProgressChanged("Restore", 70, "Memproses data...");

            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            // psql may return warnings, check for actual errors
            if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error) && error.Contains("ERROR"))
            {
                throw new InvalidOperationException($"Restore gagal: {error}");
            }

            result.Success = true;
            OnProgressChanged("Restore", 100, "Restore berhasil!", true);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            OnProgressChanged("Restore", 0, $"Restore gagal: {ex.Message}", true, true);
        }

        return result;
    }

    /// <summary>
    /// Get available backup files
    /// </summary>
    public Task<IEnumerable<BackupFileInfo>> GetAvailableBackupsAsync()
    {
        var backups = new List<BackupFileInfo>();

        if (Directory.Exists(_backupDirectory))
        {
            var files = Directory.GetFiles(_backupDirectory, "smartkasir_backup_*.sql")
                .OrderByDescending(f => f);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                backups.Add(new BackupFileInfo
                {
                    FileName = fileInfo.Name,
                    FilePath = fileInfo.FullName,
                    FileSizeBytes = fileInfo.Length,
                    CreatedAt = fileInfo.CreationTime
                });
            }
        }

        return Task.FromResult<IEnumerable<BackupFileInfo>>(backups);
    }

    /// <summary>
    /// Delete backup file
    /// </summary>
    public Task<bool> DeleteBackupAsync(string backupFilePath)
    {
        try
        {
            if (File.Exists(backupFilePath))
            {
                File.Delete(backupFilePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    #region Helper Methods

    private async Task RunPsqlCommandAsync(string psqlPath, string arguments)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = psqlPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            Environment = { ["PGPASSWORD"] = _password }
        };

        using var process = new Process { StartInfo = processInfo };
        process.Start();
        await process.WaitForExitAsync();
    }

    private static string? FindPgDump()
    {
        return FindPostgresExecutable("pg_dump.exe");
    }

    private static string? FindPsql()
    {
        return FindPostgresExecutable("psql.exe");
    }

    private static string? FindPostgresExecutable(string executableName)
    {
        // Check PATH first
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (var path in pathEnv.Split(Path.PathSeparator))
        {
            var fullPath = Path.Combine(path, executableName);
            if (File.Exists(fullPath))
                return fullPath;
        }

        // Check common PostgreSQL installation paths
        var commonPaths = new[]
        {
            @"C:\Program Files\PostgreSQL\16\bin",
            @"C:\Program Files\PostgreSQL\15\bin",
            @"C:\Program Files\PostgreSQL\14\bin",
            @"C:\Program Files\PostgreSQL\13\bin",
            @"C:\Program Files (x86)\PostgreSQL\16\bin",
            @"C:\Program Files (x86)\PostgreSQL\15\bin"
        };

        foreach (var basePath in commonPaths)
        {
            var fullPath = Path.Combine(basePath, executableName);
            if (File.Exists(fullPath))
                return fullPath;
        }

        return null;
    }

    private void OnProgressChanged(string operation, int percent, string? message, 
        bool isComplete = false, bool hasError = false)
    {
        ProgressChanged?.Invoke(this, new BackupProgressEventArgs
        {
            Operation = operation,
            ProgressPercent = percent,
            Message = message,
            IsComplete = isComplete,
            HasError = hasError
        });
    }

    #endregion
}
