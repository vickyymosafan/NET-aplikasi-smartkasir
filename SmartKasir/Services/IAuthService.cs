using SmartKasir.Application.DTOs;

namespace SmartKasir.Client.Services;

/// <summary>
/// Service untuk autentikasi pengguna di client
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Login dengan username dan password
    /// </summary>
    Task<ClientAuthResult> LoginAsync(string username, string password);

    /// <summary>
    /// Logout dan invalidate token
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    Task<bool> RefreshTokenAsync();

    /// <summary>
    /// Check apakah user sudah authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Get current user
    /// </summary>
    ClientUserDto? CurrentUser { get; }

    /// <summary>
    /// Get current JWT token
    /// </summary>
    string? CurrentToken { get; }

    /// <summary>
    /// Event ketika authentication status berubah
    /// </summary>
    event EventHandler<AuthStatusChangedEventArgs>? AuthStatusChanged;
}

#region Client-specific Auth Types

/// <summary>
/// Event args untuk authentication status change (client-specific)
/// </summary>
public class AuthStatusChangedEventArgs : EventArgs
{
    public bool IsAuthenticated { get; set; }
    public ClientUserDto? User { get; set; }
}

/// <summary>
/// Result dari operasi autentikasi di client
/// Berbeda dari AuthResult di Application layer karena menggunakan ClientUserDto
/// </summary>
public record ClientAuthResult(
    bool Success,
    string? Token,
    string? RefreshToken,
    ClientUserDto? User,
    string? ErrorMessage);

/// <summary>
/// DTO untuk user di client dengan helper properties
/// Extends UserDto dari Application layer dengan client-specific functionality
/// </summary>
public record ClientUserDto(
    Guid Id,
    string Username,
    SmartKasir.Core.Enums.UserRole Role,
    bool IsActive)
{
    public bool IsAdmin => Role == SmartKasir.Core.Enums.UserRole.Admin;
    public bool IsCashier => Role == SmartKasir.Core.Enums.UserRole.Cashier;
    
    /// <summary>
    /// Convert dari Application UserDto ke ClientUserDto
    /// </summary>
    public static ClientUserDto? FromUserDto(UserDto? dto) =>
        dto == null ? null : new ClientUserDto(dto.Id, dto.Username, dto.Role, dto.IsActive);
}

#endregion
