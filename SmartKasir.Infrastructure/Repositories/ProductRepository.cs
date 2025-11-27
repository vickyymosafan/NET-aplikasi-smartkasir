using Microsoft.EntityFrameworkCore;
using SmartKasir.Core.Entities;
using SmartKasir.Core.Interfaces;
using SmartKasir.Infrastructure.Persistence;

namespace SmartKasir.Infrastructure.Repositories;

/// <summary>
/// Repository implementation untuk Product entity
/// </summary>
public class ProductRepository : EfRepository<Product>, IProductRepository
{
    public ProductRepository(SmartKasirDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode)
    {
        return await _dbSet
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Barcode == barcode);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string keyword)
    {
        var lowerKeyword = keyword.ToLower();
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.IsActive && 
                (p.Name.ToLower().Contains(lowerKeyword) || 
                 p.Barcode.ToLower().Contains(lowerKeyword)))
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync();
    }

    public async Task<bool> BarcodeExistsAsync(string barcode)
    {
        return await _dbSet
            .AnyAsync(p => p.Barcode == barcode);
    }

    public async Task<bool> BarcodeExistsAsync(string barcode, Guid excludeProductId)
    {
        return await _dbSet
            .AnyAsync(p => p.Barcode == barcode && p.Id != excludeProductId);
    }
}
