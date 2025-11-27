using SmartKasir.Application.DTOs;

namespace SmartKasir.Application.Services;

/// <summary>
/// Interface untuk layanan user
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Mendapatkan semua user
    /// </summary>
    Task<IEnumerable<UserDto>> GetAllAsync();

    /// <summary>
    /// Mendapatkan user berdasarkan ID
    /// </summary>
    Task<UserDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Membuat user baru
    /// </summary>
    Task<OperationResult<UserDto>> CreateAsync(CreateUserRequest request);

    /// <summary>
    /// Memperbarui user
    /// </summary>
    Task<OperationResult<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request);

    /// <summary>
    /// Menonaktifkan user
    /// </summary>
    Task<OperationResult> DeactivateAsync(Guid id);

    /// <summary>
    /// Mengaktifkan user
    /// </summary>
    Task<OperationResult> ActivateAsync(Guid id);

    /// <summary>
    /// Reset password user
    /// </summary>
    Task<OperationResult> ResetPasswordAsync(Guid id, ResetPasswordRequest request);
}
