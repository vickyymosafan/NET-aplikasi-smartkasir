using SmartKasir.Core.Enums;

namespace SmartKasir.Application.DTOs;

/// <summary>
/// DTO untuk menampilkan data user
/// </summary>
public record UserDto(Guid Id, string Username, UserRole Role, bool IsActive, DateTime CreatedAt);

/// <summary>
/// Request untuk membuat user baru
/// </summary>
public record CreateUserRequest(string Username, string Password, UserRole Role);

/// <summary>
/// Request untuk update user
/// </summary>
public record UpdateUserRequest(string? Username, UserRole? Role, bool? IsActive);

/// <summary>
/// Request untuk reset password
/// </summary>
public record ResetPasswordRequest(string NewPassword);
