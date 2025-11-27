namespace SmartKasir.Application.DTOs;

/// <summary>
/// Request untuk login
/// </summary>
public record LoginRequest(string Username, string Password);

/// <summary>
/// Response autentikasi berhasil
/// </summary>
public record AuthResponse(string Token, string RefreshToken, UserDto User);

/// <summary>
/// Request untuk refresh token
/// </summary>
public record RefreshTokenRequest(string RefreshToken);

/// <summary>
/// Result dari operasi autentikasi
/// </summary>
public record AuthResult(bool Success, string? Token, string? RefreshToken, UserDto? User, string? ErrorMessage);
