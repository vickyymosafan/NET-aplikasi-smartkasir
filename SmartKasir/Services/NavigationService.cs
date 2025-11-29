using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace SmartKasir.Client.Services;

/// <summary>
/// Implementation dari INavigationService
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private ContentControl? _mainContent;
    private string? _currentView;
    private readonly Stack<string> _navigationStack = new();

    public event EventHandler<NavigationEventArgs>? NavigationOccurred;

    public string? CurrentView => _currentView;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void SetMainContent(ContentControl contentControl)
    {
        _mainContent = contentControl;
    }

    public void NavigateTo<T>(object? parameter = null) where T : class
    {
        var viewName = typeof(T).Name;
        
        if (!string.IsNullOrEmpty(_currentView))
        {
            _navigationStack.Push(_currentView);
        }

        _currentView = viewName;

        // Get view from DI container
        var view = _serviceProvider.GetService<T>();
        
        if (view != null && _mainContent != null)
        {
            _mainContent.Content = view;
        }

        OnNavigationOccurred(new NavigationEventArgs
        {
            ViewName = viewName,
            Parameter = parameter
        });
    }

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
