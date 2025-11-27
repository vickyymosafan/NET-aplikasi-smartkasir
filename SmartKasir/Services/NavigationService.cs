namespace SmartKasir.Client.Services;

/// <summary>
/// Implementation dari INavigationService
/// </summary>
public class NavigationService : INavigationService
{
    private string? _currentView;
    private readonly Stack<string> _navigationStack = new();

    public event EventHandler<NavigationEventArgs>? NavigationOccurred;

    public string? CurrentView => _currentView;

    public void NavigateTo(string viewName, object? parameter = null)
    {
        if (!string.IsNullOrEmpty(_currentView))
        {
            _navigationStack.Push(_currentView);
        }

        _currentView = viewName;

        OnNavigationOccurred(new NavigationEventArgs
        {
            ViewName = viewName,
            Parameter = parameter
        });
    }

    public void GoBack()
    {
        if (_navigationStack.Count > 0)
        {
            _currentView = _navigationStack.Pop();

            OnNavigationOccurred(new NavigationEventArgs
            {
                ViewName = _currentView ?? string.Empty,
                Parameter = null
            });
        }
    }

    private void OnNavigationOccurred(NavigationEventArgs args)
    {
        NavigationOccurred?.Invoke(this, args);
    }
}
