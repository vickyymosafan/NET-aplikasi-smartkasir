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
        Console.WriteLine($"[NavigationService] NavigateTo<{viewName}> called");
        
        if (!string.IsNullOrEmpty(_currentView))
        {
            _navigationStack.Push(_currentView);
        }

        _currentView = viewName;

        // Get view from DI container
        Console.WriteLine($"[NavigationService] Resolving {viewName} from DI container");
        try
        {
            var view = _serviceProvider.GetService<T>();
            Console.WriteLine($"[NavigationService] View resolved: {view != null}");
            Console.WriteLine($"[NavigationService] View type: {view?.GetType().FullName}");
            Console.WriteLine($"[NavigationService] MainContent exists: {_mainContent != null}");
            
            if (view != null && _mainContent != null)
            {
                Console.WriteLine($"[NavigationService] Setting MainContent.Content to {viewName}");
                _mainContent.Content = view;
                Console.WriteLine($"[NavigationService] MainContent.Content set successfully");
            }
            else
            {
                Console.WriteLine($"[NavigationService] ERROR: view is null or _mainContent is null");
                Console.WriteLine($"[NavigationService]   view == null: {view == null}");
                Console.WriteLine($"[NavigationService]   _mainContent == null: {_mainContent == null}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NavigationService] ERROR resolving view: {ex.Message}");
            Console.WriteLine($"[NavigationService] Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[NavigationService] Inner exception: {ex.InnerException.Message}");
                Console.WriteLine($"[NavigationService] Inner stack: {ex.InnerException.StackTrace}");
            }
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
