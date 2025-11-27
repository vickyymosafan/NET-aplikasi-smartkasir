using AutoMapper;
using SmartKasir.Application.DTOs;
using SmartKasir.Core.Entities;
using SmartKasir.Core.Interfaces;

namespace SmartKasir.Application.Services;

/// <summary>
/// Implementasi layanan kategori
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        return category != null ? _mapper.Map<CategoryDto>(category) : null;
    }

    public async Task<OperationResult<CategoryDto>> CreateAsync(CreateCategoryRequest request)
    {
        // Cek nama duplikat
        if (await _unitOfWork.Categories.NameExistsAsync(request.Name))
        {
            return new OperationResult<CategoryDto>(false, null, "Nama kategori sudah digunakan");
        }

        var category = new Category(request.Name);
        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        var categoryDto = _mapper.Map<CategoryDto>(category);
        return new OperationResult<CategoryDto>(true, categoryDto, "Kategori berhasil dibuat");
    }

    public async Task<OperationResult<CategoryDto>> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            return new OperationResult<CategoryDto>(false, null, "Kategori tidak ditemukan");
        }

        // Cek nama duplikat (kecuali untuk kategori ini sendiri)
        if (await _unitOfWork.Categories.NameExistsAsync(request.Name, id))
        {
            return new OperationResult<CategoryDto>(false, null, "Nama kategori sudah digunakan");
        }

        category.UpdateName(request.Name);
        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        var categoryDto = _mapper.Map<CategoryDto>(category);
        return new OperationResult<CategoryDto>(true, categoryDto, "Kategori berhasil diperbarui");
    }

    public async Task<OperationResult> DeleteAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            return new OperationResult(false, "Kategori tidak ditemukan");
        }

        // Cek apakah kategori memiliki produk
        if (await _unitOfWork.Categories.HasProductsAsync(id))
        {
            return new OperationResult(false, "Kategori tidak dapat dihapus karena masih memiliki produk");
        }

        await _unitOfWork.Categories.DeleteAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return new OperationResult(true, "Kategori berhasil dihapus");
    }
}
