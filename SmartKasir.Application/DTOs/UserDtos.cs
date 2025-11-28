using SmartKasir.Core.Enums;

namespace SmartKasir.Application.DTOs;

/// <summary>
/// DTO untuk menampilkan data user
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request untuk membuat user baru
/// </summary>
public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

/// <summary>
/// Request untuk update user
/// </summary>
public class UpdateUserRequest
{
    public string? Username { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Request untuk reset password
/// </summary>
public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
