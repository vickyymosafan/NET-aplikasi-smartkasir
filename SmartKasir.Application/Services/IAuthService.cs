using SmartKasir.Application.DTOs;

namespace SmartKasir.Application.Services;

/// <summary>
/// Interface untuk layanan autentikasi
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Login dengan username dan password
    /// </summary>
    Task<AuthResult> LoginAsync(string username, string password);

    /// <summary>
    /// Logout dan invalidasi token
    /// </summary>
    Task LogoutAsync(string token);

    /// <summary>
    /// Refresh access token menggunakan refresh token
    /// </summary>
    Task<AuthResult> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Validasi JWT token
    /// </summary>
    Task<bool> ValidateTokenAsync(string token);

    /// <summary>
    /// Mendapatkan user dari token
    /// </summary>
    Task<UserDto?> GetUserFromTokenAsync(string token);
}
