using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKasir.Application.DTOs;
using SmartKasir.Application.Services;

namespace SmartKasir.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Mendapatkan semua produk dengan pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll([FromQuery] PaginationParams pagination)
    {
        var result = await _productService.GetAllAsync(pagination);
        return Ok(result);
    }

    /// <summary>
    /// Mendapatkan produk berdasarkan barcode
    /// </summary>
    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<ProductDto>> GetByBarcode(string barcode)
    {
        var product = await _productService.GetByBarcodeAsync(barcode);
        if (product == null)
        {
            return NotFound(new { message = "Produk tidak ditemukan" });
        }
        return Ok(product);
    }

    /// <summary>
    /// Mendapatkan produk berdasarkan ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound(new { message = "Produk tidak ditemukan" });
        }
        return Ok(product);
    }


    /// <summary>
    /// Mencari produk berdasarkan keyword
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Search([FromQuery] string keyword)
    {
        var products = await _productService.SearchAsync(keyword);
        return Ok(products);
    }

    /// <summary>
    /// Mendapatkan semua produk aktif
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetActiveProducts()
    {
        var products = await _productService.GetActiveProductsAsync();
        return Ok(products);
    }

    /// <summary>
    /// Membuat produk baru (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request)
    {
        var result = await _productService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Memperbarui produk (Admin only)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var result = await _productService.UpdateAsync(id, request);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Menonaktifkan produk / soft delete (Admin only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Deactivate(Guid id)
    {
        var result = await _productService.DeactivateAsync(id);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(new { message = result.Message });
    }
}
