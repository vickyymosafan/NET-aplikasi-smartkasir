using AutoMapper;
using SmartKasir.Application.DTOs;
using SmartKasir.Core.Entities;
using SmartKasir.Core.Enums;
using SmartKasir.Core.Interfaces;

namespace SmartKasir.Application.Services;

/// <summary>
/// Implementasi layanan transaksi
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TransactionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TransactionResult> ProcessSaleAsync(Guid cashierId, CreateTransactionRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Validasi dan hitung total
            decimal totalAmount = 0;
            var transactionItems = new List<TransactionItem>();

            foreach (var item in request.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return new TransactionResult(false, null, 0, $"Produk dengan ID {item.ProductId} tidak ditemukan");
                }

                if (!product.IsActive)
                {
                    await _unitOfWork.RollbackAsync();
                    return new TransactionResult(false, null, 0, $"Produk {product.Name} tidak aktif");
                }

                if (!product.HasSufficientStock(item.Quantity))
                {
                    await _unitOfWork.RollbackAsync();
                    return new TransactionResult(false, null, 0, $"Stok {product.Name} tidak cukup");
                }

                // Kurangi stok
                product.DecrementStock(item.Quantity);
                await _unitOfWork.Products.UpdateAsync(product);

                // Buat transaction item dengan price snapshot
                var transactionItem = new TransactionItem(
                    item.ProductId,
                    item.Quantity,
                    product.Price);

                transactionItems.Add(transactionItem);
                totalAmount += transactionItem.Subtotal;
            }

            // Validasi pembayaran untuk cash
            decimal change = 0;
            if (request.PaymentMethod == PaymentMethod.Cash)
            {
                if (request.AmountPaid < totalAmount)
                {
                    await _unitOfWork.RollbackAsync();
                    return new TransactionResult(false, null, 0, "Jumlah pembayaran kurang dari total");
                }
                change = request.AmountPaid - totalAmount;
            }

            // Generate invoice number
            var invoiceNumber = await GenerateInvoiceNumberAsync();

            // Buat transaksi
            var transaction = new Transaction(
                invoiceNumber,
                cashierId,
                totalAmount,
                0, // Tax amount - bisa dikonfigurasi
                request.PaymentMethod);

            foreach (var item in transactionItems)
            {
                transaction.AddItem(item);
            }

            await _unitOfWork.Transactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            var transactionDto = _mapper.Map<TransactionDto>(transaction);
            return new TransactionResult(true, transactionDto, change, null);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return new TransactionResult(false, null, 0, $"Terjadi kesalahan: {ex.Message}");
        }
    }

    public async Task<TransactionDto?> GetByIdAsync(Guid id)
    {
        var transaction = await _unitOfWork.Transactions.GetByIdAsync(id);
        return transaction != null ? _mapper.Map<TransactionDto>(transaction) : null;
    }

    public async Task<TransactionDto?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        var transaction = await _unitOfWork.Transactions.GetByInvoiceNumberAsync(invoiceNumber);
        return transaction != null ? _mapper.Map<TransactionDto>(transaction) : null;
    }

    public async Task<PagedResult<TransactionDto>> GetAllAsync(TransactionFilterParams filter)
    {
        IEnumerable<Transaction> transactions;

        if (filter.StartDate.HasValue && filter.EndDate.HasValue)
        {
            transactions = await _unitOfWork.Transactions.GetByDateRangeAsync(
                filter.StartDate.Value, filter.EndDate.Value);
        }
        else if (filter.CashierId.HasValue)
        {
            transactions = await _unitOfWork.Transactions.GetByCashierAsync(filter.CashierId.Value);
        }
        else
        {
            transactions = await _unitOfWork.Transactions.GetAllAsync();
        }

        var transactionList = transactions.ToList();
        var totalCount = transactionList.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        var pagedTransactions = transactionList
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        var transactionDtos = _mapper.Map<List<TransactionDto>>(pagedTransactions);
        return new PagedResult<TransactionDto>(transactionDtos, totalCount, filter.Page, filter.PageSize, totalPages);
    }

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var sequence = await _unitOfWork.Transactions.GetTodaySequenceNumberAsync();
        return Transaction.GenerateInvoiceNumber(sequence + 1);
    }
}
