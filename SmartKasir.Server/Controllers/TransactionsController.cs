using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKasir.Application.DTOs;
using SmartKasir.Application.Services;

namespace SmartKasir.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Membuat transaksi baru
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TransactionDto>> Create([FromBody] CreateTransactionRequest request)
    {
        var cashierIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(cashierIdClaim) || !Guid.TryParse(cashierIdClaim, out var cashierId))
        {
            return Unauthorized(new { message = "User tidak valid" });
        }

        var result = await _transactionService.ProcessSaleAsync(cashierId, request);
        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Transaction!.Id }, new
        {
            transaction = result.Transaction,
            change = result.Change
        });
    }

    /// <summary>
    /// Mendapatkan transaksi berdasarkan ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionDto>> GetById(Guid id)
    {
        var transaction = await _transactionService.GetByIdAsync(id);
        if (transaction == null)
        {
            return NotFound(new { message = "Transaksi tidak ditemukan" });
        }
        return Ok(transaction);
    }


    /// <summary>
    /// Mendapatkan transaksi berdasarkan invoice number
    /// </summary>
    [HttpGet("invoice/{invoiceNumber}")]
    public async Task<ActionResult<TransactionDto>> GetByInvoiceNumber(string invoiceNumber)
    {
        var transaction = await _transactionService.GetByInvoiceNumberAsync(invoiceNumber);
        if (transaction == null)
        {
            return NotFound(new { message = "Transaksi tidak ditemukan" });
        }
        return Ok(transaction);
    }

    /// <summary>
    /// Mendapatkan semua transaksi dengan filter (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<TransactionDto>>> GetAll([FromQuery] TransactionFilterParams filter)
    {
        var result = await _transactionService.GetAllAsync(filter);
        return Ok(result);
    }
}
