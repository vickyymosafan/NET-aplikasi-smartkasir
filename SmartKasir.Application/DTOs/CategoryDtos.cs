namespace SmartKasir.Application.DTOs;

/// <summary>
/// DTO untuk menampilkan data kategori
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}

/// <summary>
/// Request untuk membuat kategori baru
/// </summary>
public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Request untuk update kategori
/// </summary>
public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
}
