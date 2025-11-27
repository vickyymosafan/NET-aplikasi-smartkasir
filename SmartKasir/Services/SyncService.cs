using Microsoft.EntityFrameworkCore;
using SmartKasir.Shared.Data;
using System.Diagnostics;

namespace SmartKasir.Client.Services;

/// <summary>
/// Service untuk sinkronisasi data offline-first
/// Implements last-write-wins conflict resolution strategy
/// </summary>
public class SyncService : ISyncService
{
    private readonly ISmartKasirApi _api;
    private readonly LocalDbContext _dbContext;
    private readonly IAuthService _authService;
    private bool _isOnline;
    private DateTime? _lastSyncTime;
    private CancellationTokenSource? _syncCancellation;
    private Timer? _connectionCheckTimer;

    public event EventHandler<SyncStatusEventArgs>? SyncStatusChanged;
    public event EventHandler<ConnectionStatusEventArgs>? ConnectionStatusChanged;

    public bool IsOnline
    {
        get => _isOnline;
        private set
        {
            if (_isOnline != value)
            {
                _isOnline = value;
                OnConnectionStatusChanged(new ConnectionStatusEventArgs
                {
                    IsOnline = value,
                    ChangedAt = DateTime.UtcNow
                });

                if (value)
                {
                    // Trigger sync ketika online
                    _ = SyncToServerAsync();
                }
            }
        }
    }

    public DateTime? LastSyncTime => _lastSyncTime;

    public int PendingTransactionCount
    {
        get
        {
            using var context = new LocalDbContext();
            return context.Transactions
                .Where(t => t.SyncStatus == SyncStatus.Pending || t.SyncStatus == SyncStatus.Failed)
                .Count();
        }
    }

    public SyncService(ISmartKasirApi api, LocalDbContext dbContext, IAuthService authService)
    {
        _api = api;
        _dbContext = dbContext;
        _authService = authService;
        _isOnline = false;

        // Start connection monitoring
        StartConnectionMonitoring();
    }

    /// <summary>
    /// Sync data ke server (push pending transactions)
    /// </summary>
    public async Task SyncToServerAsync()
    {
        if (!IsOnline || !_authService.IsAuthenticated)
        {
            return;
        }

        _syncCancellation = new CancellationTokenSource();

        try
        {
            OnSyncStatusChanged(new SyncStatusEventArgs
            {
                Status = SyncStatusType.Syncing,
                Message = "Syncing to server..."
            });

            var pendingTransactions = await _dbContext.Transactions
                .Where(t => t.SyncStatus == SyncStatus.Pending || t.SyncStatus == SyncStatus.Failed)
                .Include(t => t.Items)
                .ToListAsync(_syncCancellation.Token);

            int totalCount = pendingTransactions.Count;
            int processedCount = 0;

            foreach (var transaction in pendingTransactions)
            {
                try
                {
                    // Convert to DTO
                    var request = new CreateTransactionRequest(
                        transaction.Items.Select(i => new TransactionItemRequest(i.ProductId, i.Quantity)).ToList(),
                        transaction.PaymentMethod,
                        transaction.TotalAmount);

                    // Send to server
                    var result = await _api.CreateTransactionAsync(request);

                    // Mark as synced
                    transaction.SyncStatus = SyncStatus.Synced;
                    transaction.SyncedAt = DateTime.UtcNow;
                    transaction.SyncError = null;

                    await _dbContext.SaveChangesAsync(_syncCancellation.Token);

                    processedCount++;
                    int percentage = (int)((processedCount / (double)totalCount) * 100);
                    OnSyncStatusChanged(new SyncStatusEventArgs
                    {
                        Status = SyncStatusType.Syncing,
                        Message = $"Syncing transaction {processedCount}/{totalCount}",
                        ProgressPercentage = percentage
                    });
                }
                catch (Exception ex)
                {
                    transaction.SyncStatus = SyncStatus.Failed;
                    transaction.SyncError = ex.Message;
                    await _dbContext.SaveChangesAsync(_syncCancellation.Token);

                    Debug.WriteLine($"Failed to sync transaction {transaction.Id}: {ex.Message}");
                }
            }

            _lastSyncTime = DateTime.UtcNow;
            OnSyncStatusChanged(new SyncStatusEventArgs
            {
                Status = SyncStatusType.Completed,
                Message = $"Sync completed: {processedCount}/{totalCount} transactions synced"
            });
        }
        catch (OperationCanceledException)
        {
            OnSyncStatusChanged(new SyncStatusEventArgs
            {
                Status = SyncStatusType.Failed,
                Message = "Sync cancelled"
            });
        }
        catch (Exception ex)
        {
            OnSyncStatusChanged(new SyncStatusEventArgs
            {
                Status = SyncStatusType.Failed,
                Message = $"Sync failed: {ex.Message}",
                Error = ex
            });
        }
        finally
        {
            _syncCancellation?.Dispose();
            _syncCancellation = null;
        }
    }

    /// <summary>
    /// Sync data dari server (pull product updates)
    /// </summary>
    public async Task SyncFromServerAsync()
    {
        if (!IsOnline || !_authService.IsAuthenticated)
        {
            return;
        }

        try
        {
            OnSyncStatusChanged(new SyncStatusEventArgs
            {
                Status = SyncStatusType.Syncing,
                Message = "Pulling product updates..."
            });

            // Get all products from server
            var products = await _api.GetProductsAsync(page: 1, pageSize: 10000);

            // Apply last-write-wins strategy
            foreach (var product in products.Items)
            {
                var localProduct = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (localProduct == null)
                {
                    // New product from server
                    _dbContext.Products.Add(new LocalProduct
                    {
                        Id = product.Id,
                        Barcode = product.Barcode,
                        Name = product.Name,
                        Price = product.Price,
                        StockQty = product.StockQty,
                        CategoryId = product.CategoryId,
                        CategoryName = product.CategoryName,
                        IsActive = product.IsActive,
                        LastSyncedAt = DateTime.UtcNow
                    });
                }
                else if (product.Price != localProduct.Price || 
                         product.StockQty != localProduct.StockQty ||
                         product.IsActive != localProduct.IsActive)
                {
                    // Update existing product (server version wins)
                    localProduct.Price = product.Price;
                    localProduct.StockQty = product.StockQty;
                    localProduct.IsActive = product.IsActive;
                    localProduct.LastSyncedAt = DateTime.UtcNow;
                }
            }

            await _dbContext.SaveChangesAsync();

            _lastSyncTime = DateTime.UtcNow;
            OnSyncStatusChanged(new SyncStatusEventArgs
            {
                Status = SyncStatusType.Completed,
                Message = $"Pulled {products.Items.Count} products from server"
            });
        }
        catch (Exception ex)
        {
            OnSyncStatusChanged(new SyncStatusEventArgs
            {
                Status = SyncStatusType.Failed,
                Message = $"Pull failed: {ex.Message}",
                Error = ex
            });
        }
    }

    /// <summary>
    /// Start monitoring connection status
    /// </summary>
    private void StartConnectionMonitoring()
    {
        _connectionCheckTimer = new Timer(async _ =>
        {
            try
            {
                // Try to reach server
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await client.GetAsync("https://localhost:5001/api/v1/health");
                IsOnline = response.IsSuccessStatusCode;
            }
            catch
            {
                IsOnline = false;
            }
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    private void OnSyncStatusChanged(SyncStatusEventArgs args)
    {
        SyncStatusChanged?.Invoke(this, args);
    }

    private void OnConnectionStatusChanged(ConnectionStatusEventArgs args)
    {
        ConnectionStatusChanged?.Invoke(this, args);
    }

    public void Dispose()
    {
        _connectionCheckTimer?.Dispose();
        _syncCancellation?.Dispose();
    }
}
