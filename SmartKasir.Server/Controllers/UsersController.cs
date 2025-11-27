using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKasir.Application.DTOs;
using SmartKasir.Application.Services;

namespace SmartKasir.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Mendapatkan semua user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    /// <summary>
    /// Mendapatkan user berdasarkan ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User tidak ditemukan" });
        }
        return Ok(user);
    }

    /// <summary>
    /// Membuat user baru
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        var result = await _userService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }


    /// <summary>
    /// Memperbarui user
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateAsync(id, request);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Menonaktifkan user
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Deactivate(Guid id)
    {
        var result = await _userService.DeactivateAsync(id);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Reset password user
    /// </summary>
    [HttpPost("{id:guid}/reset-password")]
    public async Task<ActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request)
    {
        var result = await _userService.ResetPasswordAsync(id, request);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(new { message = result.Message });
    }
}
