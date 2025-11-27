namespace SmartKasir.Core.Enums;

/// <summary>
/// Peran pengguna dalam sistem SmartKasir
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Administrator dengan akses penuh ke semua fitur
    /// </summary>
    Admin,
    
    /// <summary>
    /// Kasir dengan akses terbatas untuk transaksi penjualan
    /// </summary>
    Cashier
}