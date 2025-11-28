namespace SmartKasir.Client.Services;

/// <summary>
/// Service untuk autentikasi pengguna
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Login dengan username dan password
    /// </summary>
    Task<AuthResult> LoginAsync(string username, string password);

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
    UserDto? CurrentUser { get; }

    /// <summary>
    /// Get current JWT token
    /// </summary>
    string? CurrentToken { get; }

    /// <summary>
    /// Event ketika authentication status berubah
    /// </summary>
    event EventHandler<AuthStatusChangedEventArgs>? AuthStatusChanged;
}

/// <summary>
/// Event args untuk authentication status change
/// </summary>
public class AuthStatusChangedEventArgs : EventArgs
{
    public bool IsAuthenticated { get; set; }
    public UserDto? User { get; set; }
}

/// <summary>
/// Result dari operasi autentikasi
/// </summary>
public record AuthResult(
    bool Success,
    string? Token,
    string? RefreshToken,
    UserDto? User,
    string? ErrorMessage);
