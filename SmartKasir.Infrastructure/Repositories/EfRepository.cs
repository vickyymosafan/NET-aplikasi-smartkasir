using Microsoft.EntityFrameworkCore;
using SmartKasir.Core.Interfaces;
using SmartKasir.Infrastructure.Persistence;

namespace SmartKasir.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation menggunakan Entity Framework Core
/// </summary>
public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly SmartKasirDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public EfRepository(SmartKasirDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(object id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task<bool> ExistsAsync(object id)
    {
        return await _dbSet.FindAsync(id) != null;
    }
}
