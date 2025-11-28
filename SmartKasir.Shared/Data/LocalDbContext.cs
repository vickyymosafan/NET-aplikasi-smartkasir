using Microsoft.EntityFrameworkCore;
using SmartKasir.Core.Enums;

namespace SmartKasir.Shared.Data;

/// <summary>
/// DbContext untuk SQLite local database (offline storage)
/// </summary>
public class LocalDbContext : DbContext
{
    private static readonly string DbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SmartKasir",
        "smartkasir.db");

    public DbSet<LocalTransaction> Transactions { get; set; } = null!;
    public DbSet<LocalTransactionItem> TransactionItems { get; set; } = null!;
    public DbSet<LocalProduct> Products { get; set; } = null!;
    public DbSet<LocalSyncLog> SyncLogs { get; set; } = null!;

    public LocalDbContext() { }

    public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(DbPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            options.UseSqlite($"Data Source={DbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure LocalTransaction
        modelBuilder.Entity<LocalTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.SyncStatus);
        });

        // Configure LocalTransactionItem
        modelBuilder.Entity<LocalTransactionItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PriceAtMoment).HasPrecision(18, 2);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.HasOne(e => e.Transaction)
                .WithMany(t => t.Items)
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure LocalProduct
        modelBuilder.Entity<LocalProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Barcode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasIndex(e => e.Barcode).IsUnique();
            entity.HasIndex(e => e.LastSyncedAt);
        });

        // Configure LocalSyncLog
        modelBuilder.Entity<LocalSyncLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.SyncStatus);
        });
    }
}

/// <summary>
/// Local representation of Transaction untuk offline storage
/// </summary>
public class LocalTransaction
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CashierId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SyncedAt { get; set; }
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
    public string? SyncError { get; set; }

    public ICollection<LocalTransactionItem> Items { get; set; } = new List<LocalTransactionItem>();
}

/// <summary>
/// Local representation of TransactionItem untuk offline storage
/// </summary>
public class LocalTransactionItem
{
    public long Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtMoment { get; set; }
    public decimal Subtotal { get; set; }

    public LocalTransaction Transaction { get; set; } = null!;
}

/// <summary>
/// Local cache of Product untuk barcode lookup
/// </summary>
public class LocalProduct
{
    public Guid Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQty { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime LastSyncedAt { get; set; }
}

/// <summary>
/// Log untuk tracking sync operations
/// </summary>
public class LocalSyncLog
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public SyncOperation Operation { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SyncedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Status sinkronisasi
/// </summary>
public enum SyncStatus
{
    Pending,
    Syncing,
    Synced,
    Failed,
    Conflict
}

/// <summary>
/// Tipe operasi sinkronisasi
/// </summary>
public enum SyncOperation
{
    Create,
    Update,
    Delete
}
