using SmartKasir.Application.DTOs;

namespace SmartKasir.Client.Services;

public interface IProductService
{
    Task<List<ProductDto>> GetProductsAsync();
    Task<ProductDto?> GetProductAsync(Guid id);
    Task<ProductDto> CreateProductAsync(ProductDto product);
    Task UpdateProductAsync(ProductDto product);
    Task DeleteProductAsync(Guid id);
}
