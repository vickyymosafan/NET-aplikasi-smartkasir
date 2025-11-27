using Microsoft.EntityFrameworkCore;
using SmartKasir.Core.Entities;
using SmartKasir.Core.Enums;
using SmartKasir.Infrastructure.Persistence.Configurations;

namespace SmartKasir.Infrastructure.Persistence;

/// <summary>
/// DbContext utama untuk SmartKasir dengan PostgreSQL
/// </summary>
public class SmartKasirDbContext : DbContext
{
    public SmartKasirDbContext(DbContextOptions<SmartKasirDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionItem> TransactionItems => Set<TransactionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionItemConfiguration());

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed admin user dengan BCrypt hash untuk password "admin123"
        // BCrypt hash generated: $2a$11$rBNdGTkJ.gao4.qLgCa0AOACzOqbPs3fRP8eFGvGJHPO0/RAK.rSi
        modelBuilder.Entity<User>().HasData(
            new
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Username = "admin",
                PasswordHash = "$2a$11$rBNdGTkJ.gao4.qLgCa0AOACzOqbPs3fRP8eFGvGJHPO0/RAK.rSi",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed default cashier user dengan password "cashier123"
        modelBuilder.Entity<User>().HasData(
            new
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Username = "kasir1",
                PasswordHash = "$2a$11$rBNdGTkJ.gao4.qLgCa0AOACzOqbPs3fRP8eFGvGJHPO0/RAK.rSi",
                Role = UserRole.Cashier,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed categories
        modelBuilder.Entity<Category>().HasData(
            new { Id = 1, Name = "Makanan" },
            new { Id = 2, Name = "Minuman" },
            new { Id = 3, Name = "Snack" },
            new { Id = 4, Name = "Kebutuhan Rumah Tangga" },
            new { Id = 5, Name = "Elektronik" }
        );
    }
}
