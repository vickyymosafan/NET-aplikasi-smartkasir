namespace SmartKasir.Application.DTOs;

/// <summary>
/// Request untuk login
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public LoginRequest() { }
    public LoginRequest(string username, string password)
    {
        Username = username;
        Password = password;
    }
}

/// <summary>
/// Response autentikasi berhasil
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto? User { get; set; }

    public AuthResponse() { }
    public AuthResponse(string token, string refreshToken, UserDto user)
    {
        Token = token;
        RefreshToken = refreshToken;
        User = user;
    }
}

/// <summary>
/// Request untuk refresh token
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;

    public RefreshTokenRequest() { }
    public RefreshTokenRequest(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}

/// <summary>
/// Result dari operasi autentikasi
/// </summary>
public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public UserDto? User { get; set; }
    public string? ErrorMessage { get; set; }

    public AuthResult() { }
    public AuthResult(bool success, string? token, string? refreshToken, UserDto? user, string? errorMessage)
    {
        Success = success;
        Token = token;
        RefreshToken = refreshToken;
        User = user;
        ErrorMessage = errorMessage;
    }
}
