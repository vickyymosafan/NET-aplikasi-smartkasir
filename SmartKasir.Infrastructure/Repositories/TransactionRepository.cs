using Microsoft.EntityFrameworkCore;
using SmartKasir.Core.Entities;
using SmartKasir.Core.Interfaces;
using SmartKasir.Infrastructure.Persistence;

namespace SmartKasir.Infrastructure.Repositories;

/// <summary>
/// Repository implementation untuk Transaction entity
/// </summary>
public class TransactionRepository : EfRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(SmartKasirDbContext context) : base(context)
    {
    }

    public override async Task<Transaction?> GetByIdAsync(object id)
    {
        if (id is Guid guidId)
        {
            return await _dbSet
                .Include(t => t.Cashier)
                .Include(t => t.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(t => t.Id == guidId);
        }
        return null;
    }

    public async Task<Transaction?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        return await _dbSet
            .Include(t => t.Cashier)
            .Include(t => t.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(t => t.InvoiceNumber == invoiceNumber);
    }

    public async Task<IEnumerable<Transaction>> GetByCashierAsync(Guid cashierId)
    {
        return await _dbSet
            .Include(t => t.Items)
            .Where(t => t.CashierId == cashierId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(t => t.Cashier)
            .Include(t => t.Items)
                .ThenInclude(i => i.Product)
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> InvoiceNumberExistsAsync(string invoiceNumber)
    {
        return await _dbSet
            .AnyAsync(t => t.InvoiceNumber == invoiceNumber);
    }

    public async Task<int> GetTodaySequenceNumberAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var todayTransactions = await _dbSet
            .Where(t => t.CreatedAt >= today && t.CreatedAt < tomorrow)
            .CountAsync();

        return todayTransactions + 1;
    }
}
