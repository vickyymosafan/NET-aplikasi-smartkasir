namespace SmartKasir.Core.Enums;

/// <summary>
/// Metode pembayaran yang didukung oleh sistem
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Pembayaran tunai
    /// </summary>
    Cash,
    
    /// <summary>
    /// Pembayaran menggunakan QRIS
    /// </summary>
    Qris,
    
    /// <summary>
    /// Pembayaran menggunakan kartu (debit/kredit)
    /// </summary>
    Card
}