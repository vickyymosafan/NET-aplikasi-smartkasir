using Microsoft.EntityFrameworkCore.Storage;
using SmartKasir.Core.Interfaces;
using SmartKasir.Infrastructure.Persistence;

namespace SmartKasir.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation untuk mengelola transaksi database
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly SmartKasirDbContext _context;
    private IDbContextTransaction? _transaction;

    private IUserRepository? _users;
    private IProductRepository? _products;
    private ICategoryRepository? _categories;
    private ITransactionRepository? _transactions;

    public UnitOfWork(SmartKasirDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => 
        _users ??= new UserRepository(_context);

    public IProductRepository Products => 
        _products ??= new ProductRepository(_context);

    public ICategoryRepository Categories => 
        _categories ??= new CategoryRepository(_context);

    public ITransactionRepository Transactions => 
        _transactions ??= new TransactionRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
