using Microsoft.EntityFrameworkCore;
using SmartKasir.Core.Entities;
using SmartKasir.Core.Interfaces;
using SmartKasir.Infrastructure.Persistence;

namespace SmartKasir.Infrastructure.Repositories;

/// <summary>
/// Repository implementation untuk Category entity
/// </summary>
public class CategoryRepository : EfRepository<Category>, ICategoryRepository
{
    public CategoryRepository(SmartKasirDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<bool> HasProductsAsync(int categoryId)
    {
        return await _context.Products
            .AnyAsync(p => p.CategoryId == categoryId);
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await _dbSet
            .AnyAsync(c => c.Name == name);
    }

    public async Task<bool> NameExistsAsync(string name, int excludeCategoryId)
    {
        return await _dbSet
            .AnyAsync(c => c.Name == name && c.Id != excludeCategoryId);
    }
}
