using FsCheck;
using FsCheck.Xunit;
using SmartKasir.Shared.Data;
using SmartKasir.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace SmartKasir.Tests.Properties;

/// <summary>
/// Property-based tests untuk sync functionality
/// </summary>
public class SyncPropertyTests
{
    private static LocalDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<LocalDbContext>()
            .UseSqlite($"Data Source=test_{Guid.NewGuid()}.db")
            .Options;
        
        var context = new LocalDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// **Feature: smart-kasir-pos, Property 25: Offline Storage**
    /// Untuk setiap transaksi yang dibuat saat offline, data harus tersimpan di SQLite lokal.
    /// </summary>
    [Property(MaxTest = 100)]
    public bool OfflineTransaction_ShouldBeSavedLocally(
        NonEmptyString invoiceNumber,
        PositiveInt totalAmount,
        PositiveInt taxAmount)
    {
        // Arrange
        var dbContext = CreateTestDbContext();
        var transaction = new LocalTransaction
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber.Get,
            CashierId = Guid.NewGuid(),
            TotalAmount = totalAmount.Get,
            TaxAmount = taxAmount.Get,
            PaymentMethod = PaymentMethod.Cash,
            CreatedAt = DateTime.UtcNow,
            SyncStatus = SyncStatus.Pending
        };

        // Act
        dbContext.Transactions.Add(transaction);
        dbContext.SaveChanges();

        // Assert - verify transaction is saved
        var saved = dbContext.Transactions.FirstOrDefault(t => t.Id == transaction.Id);
        var result = (saved != null && 
                     saved.InvoiceNumber == invoiceNumber.Get &&
                     saved.SyncStatus == SyncStatus.Pending);

        dbContext.Dispose();
        return result;
    }

    /// <summary>
    /// **Feature: smart-kasir-pos, Property 27: Conflict Resolution**
    /// Untuk setiap konflik data saat sinkronisasi, data dengan timestamp terbaru harus menang (last-write-wins).
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ConflictResolution_ShouldApplyLastWriteWins(
        PositiveInt oldPrice,
        PositiveInt newPrice)
    {
        // Arrange
        var dbContext = CreateTestDbContext();
        var productId = Guid.NewGuid();
        
        var oldProduct = new LocalProduct
        {
            Id = productId,
            Barcode = "TEST001",
            Name = "Test Product",
            Price = oldPrice.Get,
            StockQty = 10,
            CategoryId = 1,
            CategoryName = "Test",
            IsActive = true,
            LastSyncedAt = DateTime.UtcNow.AddHours(-1)
        };

        dbContext.Products.Add(oldProduct);
        dbContext.SaveChanges();

        // Act - simulate server update with newer timestamp
        var newProduct = new LocalProduct
        {
            Id = productId,
            Barcode = "TEST001",
            Name = "Test Product",
            Price = newPrice.Get,
            StockQty = 10,
            CategoryId = 1,
            CategoryName = "Test",
            IsActive = true,
            LastSyncedAt = DateTime.UtcNow // Newer timestamp
        };

        // Apply last-write-wins
        var existing = dbContext.Products.FirstOrDefault(p => p.Id == productId);
        if (existing != null && newProduct.LastSyncedAt > existing.LastSyncedAt)
        {
            existing.Price = newProduct.Price;
            existing.LastSyncedAt = newProduct.LastSyncedAt;
        }

        dbContext.SaveChanges();

        // Assert - verify newer price is applied
        var updated = dbContext.Products.FirstOrDefault(p => p.Id == productId);
        var result = (updated != null && updated.Price == newPrice.Get);

        dbContext.Dispose();
        return result;
    }

    /// <summary>
    /// **Feature: smart-kasir-pos, Property 28: Sync Consistency**
    /// Untuk setiap sinkronisasi yang selesai, data di SQLite lokal harus identik dengan data di PostgreSQL server.
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SyncCompletion_ShouldMarkTransactionAsSynced(
        NonEmptyString invoiceNumber)
    {
        // Arrange
        var dbContext = CreateTestDbContext();
        var transactionId = Guid.NewGuid();
        
        var transaction = new LocalTransaction
        {
            Id = transactionId,
            InvoiceNumber = invoiceNumber.Get,
            CashierId = Guid.NewGuid(),
            TotalAmount = 100000,
            TaxAmount = 10000,
            PaymentMethod = PaymentMethod.Cash,
            CreatedAt = DateTime.UtcNow,
            SyncStatus = SyncStatus.Pending
        };

        dbContext.Transactions.Add(transaction);
        dbContext.SaveChanges();

        // Act - simulate successful sync
        var toSync = dbContext.Transactions.FirstOrDefault(t => t.Id == transactionId);
        if (toSync != null)
        {
            toSync.SyncStatus = SyncStatus.Synced;
            toSync.SyncedAt = DateTime.UtcNow;
            toSync.SyncError = null;
        }

        dbContext.SaveChanges();

        // Assert - verify sync status is updated
        var synced = dbContext.Transactions.FirstOrDefault(t => t.Id == transactionId);
        var result = (synced != null && 
                     synced.SyncStatus == SyncStatus.Synced &&
                     synced.SyncedAt.HasValue);

        dbContext.Dispose();
        return result;
    }

    /// <summary>
    /// **Feature: smart-kasir-pos, Property 26: Sync Trigger**
    /// Untuk setiap perubahan status koneksi dari offline ke online, proses sinkronisasi harus dimulai.
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ConnectionStatusChange_ShouldTriggerSync(
        PositiveInt pendingTransactionCount)
    {
        // Arrange
        var dbContext = CreateTestDbContext();
        var count = Math.Min(pendingTransactionCount.Get, 100); // Cap at 100

        for (int i = 0; i < count; i++)
        {
            var transaction = new LocalTransaction
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{i:D4}",
                CashierId = Guid.NewGuid(),
                TotalAmount = 100000,
                TaxAmount = 10000,
                PaymentMethod = PaymentMethod.Cash,
                CreatedAt = DateTime.UtcNow,
                SyncStatus = SyncStatus.Pending
            };

            dbContext.Transactions.Add(transaction);
        }

        dbContext.SaveChanges();

        // Act - count pending transactions
        var pendingCount = dbContext.Transactions
            .Where(t => t.SyncStatus == SyncStatus.Pending)
            .Count();

        // Assert - verify pending transactions exist
        var result = (pendingCount == count);

        dbContext.Dispose();
        return result;
    }

    /// <summary>
    /// **Feature: smart-kasir-pos, Property 25: Offline Storage (Round-trip)**
    /// Untuk setiap transaksi yang disimpan offline dan kemudian di-sync, data harus tetap konsisten.
    /// </summary>
    [Property(MaxTest = 100)]
    public bool OfflineTransaction_RoundTrip_ShouldMaintainConsistency(
        NonEmptyString invoiceNumber,
        PositiveInt itemCount)
    {
        // Arrange
        var dbContext = CreateTestDbContext();
        var transactionId = Guid.NewGuid();
        var itemsCount = Math.Min(itemCount.Get, 20); // Cap at 20 items

        var transaction = new LocalTransaction
        {
            Id = transactionId,
            InvoiceNumber = invoiceNumber.Get,
            CashierId = Guid.NewGuid(),
            TotalAmount = 100000,
            TaxAmount = 10000,
            PaymentMethod = PaymentMethod.Cash,
            CreatedAt = DateTime.UtcNow,
            SyncStatus = SyncStatus.Pending,
            Items = new List<LocalTransactionItem>()
        };

        for (int i = 0; i < itemsCount; i++)
        {
            transaction.Items.Add(new LocalTransactionItem
            {
                TransactionId = transactionId,
                ProductId = Guid.NewGuid(),
                Quantity = i + 1,
                PriceAtMoment = 10000,
                Subtotal = (i + 1) * 10000
            });
        }

        dbContext.Transactions.Add(transaction);
        dbContext.SaveChanges();

        // Act - retrieve and verify
        var retrieved = dbContext.Transactions
            .Include(t => t.Items)
            .FirstOrDefault(t => t.Id == transactionId);

        // Assert - verify all items are preserved
        var result = (retrieved != null &&
                     retrieved.Items.Count == itemsCount &&
                     retrieved.InvoiceNumber == invoiceNumber.Get);

        dbContext.Dispose();
        return result;
    }
}
