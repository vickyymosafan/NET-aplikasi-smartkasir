using AutoMapper;
using SmartKasir.Application.DTOs;
using SmartKasir.Core.Entities;
using SmartKasir.Core.Interfaces;

namespace SmartKasir.Application.Services;

/// <summary>
/// Implementasi layanan produk
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto?> GetByBarcodeAsync(string barcode)
    {
        var product = await _unitOfWork.Products.GetByBarcodeAsync(barcode);
        return product != null ? _mapper.Map<ProductDto>(product) : null;
    }

    public async Task<IEnumerable<ProductDto>> SearchAsync(string keyword)
    {
        var products = await _unitOfWork.Products.SearchAsync(keyword);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
    {
        var products = await _unitOfWork.Products.GetActiveProductsAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<PagedResult<ProductDto>> GetAllAsync(PaginationParams pagination)
    {
        var allProducts = await _unitOfWork.Products.GetAllAsync();
        var productList = allProducts.ToList();
        var totalCount = productList.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize);

        var pagedProducts = productList
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        var productDtos = _mapper.Map<List<ProductDto>>(pagedProducts);
        return new PagedResult<ProductDto>(productDtos, totalCount, pagination.Page, pagination.PageSize, totalPages);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        return product != null ? _mapper.Map<ProductDto>(product) : null;
    }

    public async Task<OperationResult<ProductDto>> CreateAsync(CreateProductRequest request)
    {
        // Cek barcode duplikat
        if (await _unitOfWork.Products.BarcodeExistsAsync(request.Barcode))
        {
            return new OperationResult<ProductDto>(false, null, "Barcode sudah digunakan");
        }

        // Cek kategori valid
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
        if (category == null)
        {
            return new OperationResult<ProductDto>(false, null, "Kategori tidak ditemukan");
        }

        var product = new Product(
            request.Barcode,
            request.Name,
            request.Price,
            request.StockQty,
            request.CategoryId);

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var productDto = _mapper.Map<ProductDto>(product);
        return new OperationResult<ProductDto>(true, productDto, "Produk berhasil dibuat");
    }

    public async Task<OperationResult<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            return new OperationResult<ProductDto>(false, null, "Produk tidak ditemukan");
        }

        if (request.Name != null)
        {
            product.UpdateName(request.Name);
        }

        if (request.Price.HasValue)
        {
            product.UpdatePrice(request.Price.Value);
        }

        if (request.StockQty.HasValue)
        {
            var diff = request.StockQty.Value - product.StockQty;
            if (diff > 0)
                product.AddStock(diff);
            else if (diff < 0)
                product.DecrementStock(-diff);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
                product.Activate();
            else
                product.Deactivate();
        }

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var productDto = _mapper.Map<ProductDto>(product);
        return new OperationResult<ProductDto>(true, productDto, "Produk berhasil diperbarui");
    }

    public async Task<OperationResult> DeactivateAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            return new OperationResult(false, "Produk tidak ditemukan");
        }

        product.Deactivate();
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return new OperationResult(true, "Produk berhasil dinonaktifkan");
    }

    public async Task<OperationResult> ActivateAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            return new OperationResult(false, "Produk tidak ditemukan");
        }

        product.Activate();
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return new OperationResult(true, "Produk berhasil diaktifkan");
    }
}
