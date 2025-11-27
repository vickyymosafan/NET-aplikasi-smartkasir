namespace SmartKasir.Application.DTOs;

/// <summary>
/// DTO untuk menampilkan data kategori
/// </summary>
public record CategoryDto(int Id, string Name, int ProductCount);

/// <summary>
/// Request untuk membuat kategori baru
/// </summary>
public record CreateCategoryRequest(string Name);

/// <summary>
/// Request untuk update kategori
/// </summary>
public record UpdateCategoryRequest(string Name);
