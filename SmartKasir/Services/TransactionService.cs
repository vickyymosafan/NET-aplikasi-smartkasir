using Microsoft.EntityFrameworkCore;
using SmartKasir.Application.DTOs;
using SmartKasir.Client.Data;

namespace SmartKasir.Client.Services;

/// <summary>
/// Implementation dari ITransactionService
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly ISmartKasirApi _api;
    private readonly LocalDbContext _dbContext;
    private readonly ISyncService _syncService;
    private readonly IAuthService _authService;

    public TransactionService(
        ISmartKasirApi api,
        LocalDbContext dbContext,
        ISyncService syncService,
        IAuthService authService)
    {
        _api = api;
        _dbContext = dbContext;
        _syncService = syncService;
        _authService = authService;
    }

    public async Task<TransactionResult> ProcessSaleAsync(CreateTransactionRequest request)
    {
        if (_syncService.IsOnline)
        {
            try
            {
                // Try to process on server
                var result = await _api.CreateTransactionAsync(request);
                
                // Also save locally for audit trail
                await SaveTransactionLocallyAsync(result, SyncStatus.Synced);
                
                return new TransactionResult(true, result, 0, null);
            }
            catch (Exception ex)
            {
                // Fall back to local processing
                return await ProcessSaleLocallyAsync(request);
            }
        }
        else
        {
            // Process locally when offline
            return await ProcessSaleLocallyAsync(request);
        }
    }

    private async Task<TransactionResult> ProcessSaleLocallyAsync(CreateTransactionRequest request)
    {
        try
        {
            var invoiceNumber = await GenerateInvoiceNumberAsync();
            var now = DateTime.UtcNow;

            var transaction = new LocalTransaction
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = invoiceNumber,
                CashierId = _authService.CurrentUser?.Id ?? Guid.Empty,
                TotalAmount = request.Items.Sum(i => i.Quantity * 0), // Will be calculated from items
                TaxAmount = 0,
                PaymentMethod = request.PaymentMethod,
                CreatedAt = now,
                SyncStatus = SyncStatus.Pending,
                Items = new List<LocalTransactionItem>()
            };

            decimal totalAmount = 0;
            foreach (var item in request.Items)
            {
                var transactionItem = new LocalTransactionItem
                {
                    TransactionId = transaction.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PriceAtMoment = 0, // Should be fetched from product
                    Subtotal = 0
                };

                transaction.Items.Add(transactionItem);
                totalAmount += transactionItem.Subtotal;
            }

            transaction.TotalAmount = totalAmount;
            transaction.TaxAmount = totalAmount * 0.1m; // 10% tax

            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            var dto = new TransactionDto(
                transaction.Id,
                transaction.InvoiceNumber,
                transaction.CashierId,
                _authService.CurrentUser?.Username ?? "Unknown",
                transaction.TotalAmount,
                transaction.TaxAmount,
                transaction.PaymentMethod,
                transaction.CreatedAt,
                transaction.Items.Select(i => new TransactionItemDto(
                    i.Id, i.ProductId, "", i.Quantity, i.PriceAtMoment, i.Subtotal)).ToList());

            var change = request.AmountPaid - transaction.TotalAmount;
            return new TransactionResult(true, dto, change, null);
        }
        catch (Exception ex)
        {
            return new TransactionResult(false, null, 0, ex.Message);
        }
    }

    private async Task SaveTransactionLocallyAsync(TransactionDto transaction, SyncStatus syncStatus)
    {
        var localTransaction = new LocalTransaction
        {
            Id = transaction.Id,
            InvoiceNumber = transaction.InvoiceNumber,
            CashierId = transaction.CashierId,
            TotalAmount = transaction.TotalAmount,
            TaxAmount = transaction.TaxAmount,
            PaymentMethod = transaction.PaymentMethod,
            CreatedAt = transaction.CreatedAt,
            SyncStatus = syncStatus,
            SyncedAt = syncStatus == SyncStatus.Synced ? DateTime.UtcNow : null,
            Items = transaction.Items.Select(i => new LocalTransactionItem
            {
                TransactionId = transaction.Id,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                PriceAtMoment = i.PriceAtMoment,
                Subtotal = i.Subtotal
            }).ToList()
        };

        _dbContext.Transactions.Add(localTransaction);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<TransactionDto?> GetTransactionAsync(Guid id)
    {
        if (_syncService.IsOnline)
        {
            try
            {
                return await _api.GetTransactionAsync(id);
            }
            catch
            {
                // Fall back to local
            }
        }

        var localTransaction = await _dbContext.Transactions
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (localTransaction == null)
        {
            return null;
        }

        return new TransactionDto(
            localTransaction.Id,
            localTransaction.InvoiceNumber,
            localTransaction.CashierId,
            _authService.CurrentUser?.Username ?? "Unknown",
            localTransaction.TotalAmount,
            localTransaction.TaxAmount,
            localTransaction.PaymentMethod,
            localTransaction.CreatedAt,
            localTransaction.Items.Select(i => new TransactionItemDto(
                i.Id, i.ProductId, "", i.Quantity, i.PriceAtMoment, i.Subtotal)).ToList());
    }

    public async Task<PagedResult<TransactionDto>> GetTransactionsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 20)
    {
        if (_syncService.IsOnline)
        {
            try
            {
                return await _api.GetTransactionsAsync(startDate, endDate, page, pageSize);
            }
            catch
            {
                // Fall back to local
            }
        }

        var query = _dbContext.Transactions.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Include(t => t.Items)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(t => new TransactionDto(
            t.Id,
            t.InvoiceNumber,
            t.CashierId,
            _authService.CurrentUser?.Username ?? "Unknown",
            t.TotalAmount,
            t.TaxAmount,
            t.PaymentMethod,
            t.CreatedAt,
            t.Items.Select(i => new TransactionItemDto(
                i.Id, i.ProductId, "", i.Quantity, i.PriceAtMoment, i.Subtotal)).ToList())).ToList();

        return new PagedResult<TransactionDto>(dtos, totalCount, page, pageSize);
    }

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var today = DateTime.Now;
        var datePrefix = today.ToString("yyyyMMdd");
        
        var lastInvoice = await _dbContext.Transactions
            .Where(t => t.InvoiceNumber.StartsWith($"INV-{datePrefix}"))
            .OrderByDescending(t => t.InvoiceNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastInvoice != null)
        {
            var lastSequence = lastInvoice.InvoiceNumber.Split('-').Last();
            if (int.TryParse(lastSequence, out var parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"INV-{datePrefix}-{sequence:D4}";
    }

    public async Task<IEnumerable<TransactionDto>> GetPendingTransactionsAsync()
    {
        var pending = await _dbContext.Transactions
            .Where(t => t.SyncStatus == SyncStatus.Pending || t.SyncStatus == SyncStatus.Failed)
            .Include(t => t.Items)
            .ToListAsync();

        return pending.Select(t => new TransactionDto(
            t.Id,
            t.InvoiceNumber,
            t.CashierId,
            _authService.CurrentUser?.Username ?? "Unknown",
            t.TotalAmount,
            t.TaxAmount,
            t.PaymentMethod,
            t.CreatedAt,
            t.Items.Select(i => new TransactionItemDto(
                i.Id, i.ProductId, "", i.Quantity, i.PriceAtMoment, i.Subtotal)).ToList()));
    }
}
