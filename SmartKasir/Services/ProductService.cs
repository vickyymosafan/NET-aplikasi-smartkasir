using Microsoft.EntityFrameworkCore;
using SmartKasir.Application.DTOs;
using SmartKasir.Shared.Data;

namespace SmartKasir.Client.Services;

/// <summary>
/// Implementation dari IProductService
/// </summary>
public class ProductService : IProductService
{
    private readonly ISmartKasirApi _api;
    private readonly LocalDbContext _dbContext;
    private readonly ISyncService _syncService;

    public ProductService(ISmartKasirApi api, LocalDbContext dbContext, ISyncService syncService)
    {
        _api = api;
        _dbContext = dbContext;
        _syncService = syncService;
    }

    public async Task<ProductDto?> GetByBarcodeAsync(string barcode)
    {
        // Try local cache first
        var localProduct = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Barcode == barcode && p.IsActive);

        if (localProduct != null)
        {
            return new ProductDto(
                localProduct.Id,
                localProduct.Barcode,
                localProduct.Name,
                localProduct.Price,
                localProduct.StockQty,
                localProduct.CategoryId,
                localProduct.CategoryName,
                localProduct.IsActive);
        }

        // If online, try server
        if (_syncService.IsOnline)
        {
            try
            {
                var product = await _api.GetProductByBarcodeAsync(barcode);
                
                // Cache it locally
                var cached = new LocalProduct
                {
                    Id = product.Id,
                    Barcode = product.Barcode,
                    Name = product.Name,
                    Price = product.Price,
                    StockQty = product.StockQty,
                    CategoryId = product.CategoryId,
                    CategoryName = product.CategoryName,
                    IsActive = product.IsActive,
                    LastSyncedAt = DateTime.UtcNow
                };

                _dbContext.Products.Add(cached);
                await _dbContext.SaveChangesAsync();

                return product;
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    public async Task<IEnumerable<ProductDto>> SearchAsync(string keyword)
    {
        // Search in local cache first
        var localResults = await _dbContext.Products
            .Where(p => p.IsActive && 
                   (p.Name.Contains(keyword) || p.Barcode.Contains(keyword)))
            .ToListAsync();

        var results = localResults.Select(p => new ProductDto(
            p.Id, p.Barcode, p.Name, p.Price, p.StockQty, p.CategoryId, p.CategoryName, p.IsActive))
            .ToList();

        // If online and local results are limited, try server
        if (_syncService.IsOnline && results.Count < 10)
        {
            try
            {
                var serverResults = await _api.SearchProductsAsync(keyword);
                
                // Merge and deduplicate
                var merged = results
                    .Concat(serverResults)
                    .DistinctBy(p => p.Id)
                    .ToList();

                return merged;
            }
            catch
            {
                return results;
            }
        }

        return results;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        // Get from local cache
        var localProducts = await _dbContext.Products
            .Where(p => p.IsActive)
            .ToListAsync();

        return localProducts.Select(p => new ProductDto(
            p.Id, p.Barcode, p.Name, p.Price, p.StockQty, p.CategoryId, p.CategoryName, p.IsActive));
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(int page = 1, int pageSize = 50)
    {
        if (_syncService.IsOnline)
        {
            try
            {
                return await _api.GetProductsAsync(page, pageSize);
            }
            catch
            {
                // Fall back to local
            }
        }

        // Get from local cache
        var query = _dbContext.Products.Where(p => p.IsActive);
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(p => new ProductDto(
            p.Id, p.Barcode, p.Name, p.Price, p.StockQty, p.CategoryId, p.CategoryName, p.IsActive))
            .ToList();

        return new PagedResult<ProductDto>(dtos, totalCount, page, pageSize);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        return await _api.CreateProductAsync(request);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        return await _api.UpdateProductAsync(id, request);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _api.DeleteProductAsync(id);
    }

    public async Task RefreshCacheAsync()
    {
        if (!_syncService.IsOnline)
        {
            return;
        }

        try
        {
            var products = await _api.GetProductsAsync(page: 1, pageSize: 10000);

            // Clear and rebuild cache
            _dbContext.Products.RemoveRange(_dbContext.Products);
            await _dbContext.SaveChangesAsync();

            foreach (var product in products.Items)
            {
                _dbContext.Products.Add(new LocalProduct
                {
                    Id = product.Id,
                    Barcode = product.Barcode,
                    Name = product.Name,
                    Price = product.Price,
                    StockQty = product.StockQty,
                    CategoryId = product.CategoryId,
                    CategoryName = product.CategoryName,
                    IsActive = product.IsActive,
                    LastSyncedAt = DateTime.UtcNow
                });
            }

            await _dbContext.SaveChangesAsync();
        }
        catch
        {
            // Ignore cache refresh errors
        }
    }
}
