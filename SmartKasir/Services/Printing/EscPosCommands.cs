namespace SmartKasir.Client.Services.Printing;

/// <summary>
/// ESC/POS command constants and builder for thermal printers
/// Requirements: 5.2
/// </summary>
public static class EscPosCommands
{
    // Control characters
    public static readonly byte[] ESC = { 0x1B };
    public static readonly byte[] GS = { 0x1D };
    public static readonly byte[] LF = { 0x0A };
    public static readonly byte[] CR = { 0x0D };

    // Initialize printer
    public static readonly byte[] Initialize = { 0x1B, 0x40 }; // ESC @

    // Text formatting
    public static readonly byte[] BoldOn = { 0x1B, 0x45, 0x01 }; // ESC E 1
    public static readonly byte[] BoldOff = { 0x1B, 0x45, 0x00 }; // ESC E 0
    public static readonly byte[] UnderlineOn = { 0x1B, 0x2D, 0x01 }; // ESC - 1
    public static readonly byte[] UnderlineOff = { 0x1B, 0x2D, 0x00 }; // ESC - 0
    public static readonly byte[] DoubleHeightOn = { 0x1B, 0x21, 0x10 }; // ESC ! 16
    public static readonly byte[] DoubleWidthOn = { 0x1B, 0x21, 0x20 }; // ESC ! 32
    public static readonly byte[] DoubleHeightWidthOn = { 0x1B, 0x21, 0x30 }; // ESC ! 48
    public static readonly byte[] NormalSize = { 0x1B, 0x21, 0x00 }; // ESC ! 0

    // Text alignment
    public static readonly byte[] AlignLeft = { 0x1B, 0x61, 0x00 }; // ESC a 0
    public static readonly byte[] AlignCenter = { 0x1B, 0x61, 0x01 }; // ESC a 1
    public static readonly byte[] AlignRight = { 0x1B, 0x61, 0x02 }; // ESC a 2

    // Paper handling
    public static readonly byte[] FeedLine = { 0x0A }; // LF
    public static readonly byte[] FeedLines3 = { 0x1B, 0x64, 0x03 }; // ESC d 3
    public static readonly byte[] FeedLines5 = { 0x1B, 0x64, 0x05 }; // ESC d 5
    public static readonly byte[] CutPaper = { 0x1D, 0x56, 0x00 }; // GS V 0 (full cut)
    public static readonly byte[] CutPaperPartial = { 0x1D, 0x56, 0x01 }; // GS V 1 (partial cut)

    // Cash drawer
    public static readonly byte[] OpenCashDrawer = { 0x1B, 0x70, 0x00, 0x19, 0xFA }; // ESC p 0 25 250

    // Character set
    public static readonly byte[] CharsetPC437 = { 0x1B, 0x74, 0x00 }; // ESC t 0
    public static readonly byte[] CharsetPC850 = { 0x1B, 0x74, 0x02 }; // ESC t 2

    /// <summary>
    /// Create feed n lines command
    /// </summary>
    public static byte[] FeedLines(int n)
    {
        return new byte[] { 0x1B, 0x64, (byte)Math.Min(n, 255) };
    }

    /// <summary>
    /// Set character size (0-7 for width, 0-7 for height)
    /// </summary>
    public static byte[] SetCharacterSize(int width, int height)
    {
        var size = (byte)(((width & 0x07) << 4) | (height & 0x07));
        return new byte[] { 0x1D, 0x21, size };
    }

    /// <summary>
    /// Set line spacing in dots
    /// </summary>
    public static byte[] SetLineSpacing(int dots)
    {
        return new byte[] { 0x1B, 0x33, (byte)Math.Min(dots, 255) };
    }

    /// <summary>
    /// Reset line spacing to default
    /// </summary>
    public static readonly byte[] DefaultLineSpacing = { 0x1B, 0x32 };
}
