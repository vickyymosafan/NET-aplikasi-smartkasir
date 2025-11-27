using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using SmartKasir.Application.DTOs;
using SmartKasir.Core.Interfaces;

namespace SmartKasir.Application.Services;

/// <summary>
/// Implementasi layanan autentikasi
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;
    private readonly HashSet<string> _invalidatedTokens = new();

    public AuthService(IUnitOfWork unitOfWork, IMapper mapper, JwtSettings jwtSettings)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(username);

        if (user == null)
        {
            return new AuthResult(false, null, null, null, "Username atau password salah");
        }

        if (!user.IsActive)
        {
            return new AuthResult(false, null, null, null, "Akun tidak aktif");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return new AuthResult(false, null, null, null, "Username atau password salah");
        }

        var token = GenerateJwtToken(user.Id, user.Username, user.Role.ToString());
        var refreshToken = GenerateRefreshToken();
        var userDto = _mapper.Map<UserDto>(user);

        return new AuthResult(true, token, refreshToken, userDto, null);
    }


    public Task LogoutAsync(string token)
    {
        _invalidatedTokens.Add(token);
        return Task.CompletedTask;
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        // Dalam implementasi nyata, refresh token disimpan di database
        // Untuk saat ini, kita generate token baru
        // TODO: Implementasi penyimpanan refresh token di database
        return await Task.FromResult(new AuthResult(false, null, null, null, "Refresh token tidak valid"));
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        if (_invalidatedTokens.Contains(token))
        {
            return Task.FromResult(false);
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<UserDto?> GetUserFromTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateJwtToken(Guid userId, string username, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

/// <summary>
/// Konfigurasi JWT
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
