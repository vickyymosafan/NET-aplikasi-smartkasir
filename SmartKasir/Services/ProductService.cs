using System.Net.Http.Json;
using SmartKasir.Application.DTOs;

namespace SmartKasir.Client.Services;

public class ProductService : IProductService
{
    private readonly HttpClient _http;
    
    // Demo products for offline/fallback
    private static readonly List<ProductDto> _demoProducts = new()
    {
        new() { Id = Guid.NewGuid(), Barcode = "001", Name = "Indomie Goreng", Price = 3500, StockQty = 100, CategoryName = "Makanan" },
        new() { Id = Guid.NewGuid(), Barcode = "002", Name = "Aqua 600ml", Price = 4000, StockQty = 50, CategoryName = "Minuman" },
        new() { Id = Guid.NewGuid(), Barcode = "003", Name = "Teh Botol Sosro", Price = 5000, StockQty = 30, CategoryName = "Minuman" },
        new() { Id = Guid.NewGuid(), Barcode = "004", Name = "Roti Tawar Sari Roti", Price = 15000, StockQty = 20, CategoryName = "Makanan" },
        new() { Id = Guid.NewGuid(), Barcode = "005", Name = "Sabun Lifebuoy", Price = 8500, StockQty = 40, CategoryName = "Toiletries" },
        new() { Id = Guid.NewGuid(), Barcode = "006", Name = "Pasta Gigi Pepsodent", Price = 12000, StockQty = 25, CategoryName = "Toiletries" },
        new() { Id = Guid.NewGuid(), Barcode = "007", Name = "Minyak Goreng Bimoli 1L", Price = 28000, StockQty = 15, CategoryName = "Bahan Pokok" },
        new() { Id = Guid.NewGuid(), Barcode = "008", Name = "Gula Pasir 1kg", Price = 16000, StockQty = 35, CategoryName = "Bahan Pokok" },
    };

    public ProductService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ProductDto>> GetProductsAsync()
    {
        try
        {
            var products = await _http.GetFromJsonAsync<List<ProductDto>>("/api/products");
            return products ?? _demoProducts;
        }
        catch
        {
            return _demoProducts;
        }
    }

    public async Task<ProductDto?> GetProductAsync(Guid id)
    {
        try
        {
            return await _http.GetFromJsonAsync<ProductDto>($"/api/products/{id}");
        }
        catch
        {
            return _demoProducts.FirstOrDefault(p => p.Id == id);
        }
    }

    public async Task<ProductDto> CreateProductAsync(ProductDto product)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/products", product);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductDto>() ?? product;
        }
        catch
        {
            product.Id = Guid.NewGuid();
            _demoProducts.Add(product);
            return product;
        }
    }

    public async Task UpdateProductAsync(ProductDto product)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"/api/products/{product.Id}", product);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            var existing = _demoProducts.FirstOrDefault(p => p.Id == product.Id);
            if (existing != null)
            {
                existing.Name = product.Name;
                existing.Price = product.Price;
                existing.StockQty = product.StockQty;
            }
        }
    }

    public async Task DeleteProductAsync(Guid id)
    {
        try
        {
            var response = await _http.DeleteAsync($"/api/products/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            var product = _demoProducts.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _demoProducts.Remove(product);
            }
        }
    }
}
