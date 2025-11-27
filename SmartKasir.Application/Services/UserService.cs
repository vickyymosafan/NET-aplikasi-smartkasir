using AutoMapper;
using SmartKasir.Application.DTOs;
using SmartKasir.Core.Entities;
using SmartKasir.Core.Interfaces;

namespace SmartKasir.Application.Services;

/// <summary>
/// Implementasi layanan user
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<OperationResult<UserDto>> CreateAsync(CreateUserRequest request)
    {
        // Cek username duplikat
        if (await _unitOfWork.Users.UsernameExistsAsync(request.Username))
        {
            return new OperationResult<UserDto>(false, null, "Username sudah digunakan");
        }

        // Hash password dengan BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User(request.Username, passwordHash, request.Role);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var userDto = _mapper.Map<UserDto>(user);
        return new OperationResult<UserDto>(true, userDto, "User berhasil dibuat");
    }


    public async Task<OperationResult<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return new OperationResult<UserDto>(false, null, "User tidak ditemukan");
        }

        if (request.Role.HasValue)
        {
            user.ChangeRole(request.Role.Value);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
                user.Activate();
            else
                user.Deactivate();
        }

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var userDto = _mapper.Map<UserDto>(user);
        return new OperationResult<UserDto>(true, userDto, "User berhasil diperbarui");
    }

    public async Task<OperationResult> DeactivateAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return new OperationResult(false, "User tidak ditemukan");
        }

        user.Deactivate();
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new OperationResult(true, "User berhasil dinonaktifkan");
    }

    public async Task<OperationResult> ActivateAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return new OperationResult(false, "User tidak ditemukan");
        }

        user.Activate();
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new OperationResult(true, "User berhasil diaktifkan");
    }

    public async Task<OperationResult> ResetPasswordAsync(Guid id, ResetPasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return new OperationResult(false, "User tidak ditemukan");
        }

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatePasswordHash(newPasswordHash);

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new OperationResult(true, "Password berhasil direset");
    }
}
