using SmartKasir.Core.Enums;

namespace SmartKasir.Core.Entities;

/// <summary>
/// Entitas pengguna sistem SmartKasir
/// </summary>
public class User
{
    /// <summary>
    /// Identifier unik pengguna
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Username untuk login
    /// </summary>
    public string Username { get; private set; } = string.Empty;

    /// <summary>
    /// Hash password menggunakan BCrypt
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Peran pengguna (Admin atau Cashier)
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Status aktif pengguna
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Waktu pembuatan akun
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Constructor untuk membuat user baru
    /// </summary>
    public User(string username, string passwordHash, UserRole role)
    {
        Id = Guid.NewGuid();
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Constructor untuk EF Core
    /// </summary>
    private User() { }

    /// <summary>
    /// Mengubah peran pengguna
    /// </summary>
    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
    }

    /// <summary>
    /// Menonaktifkan pengguna
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Mengaktifkan pengguna
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Mengubah password hash
    /// </summary>
    public void UpdatePasswordHash(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }
}
