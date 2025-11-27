using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKasir.Application.DTOs;
using SmartKasir.Application.Services;

namespace SmartKasir.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Login dengan username dan password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Username, request.Password);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        return Ok(new AuthResponse(result.Token!, result.RefreshToken!, result.User!));
    }

    /// <summary>
    /// Refresh access token menggunakan refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        return Ok(new AuthResponse(result.Token!, result.RefreshToken!, result.User!));
    }


    /// <summary>
    /// Logout dan invalidasi token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        await _authService.LogoutAsync(token);
        return Ok(new { message = "Logout berhasil" });
    }

    /// <summary>
    /// Mendapatkan informasi user yang sedang login
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var user = await _authService.GetUserFromTokenAsync(token);

        if (user == null)
        {
            return Unauthorized(new { message = "Token tidak valid" });
        }

        return Ok(user);
    }
}
