namespace SmartKasir.Client.Services;

/// <summary>
/// Service untuk navigasi antar views
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigate ke view tertentu
    /// </summary>
    void NavigateTo(string viewName, object? parameter = null);

    /// <summary>
    /// Navigate back ke view sebelumnya
    /// </summary>
    void GoBack();

    /// <summary>
    /// Get current view name
    /// </summary>
    string? CurrentView { get; }

    /// <summary>
    /// Event ketika navigation terjadi
    /// </summary>
    event EventHandler<NavigationEventArgs>? NavigationOccurred;
}

/// <summary>
/// Event args untuk navigation
/// </summary>
public class NavigationEventArgs : EventArgs
{
    public string ViewName { get; set; } = string.Empty;
    public object? Parameter { get; set; }
}
