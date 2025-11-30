using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using SmartKasir.Client.Services;
using System.Windows.Forms;

namespace SmartKasir.Client;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();
        
        // Configure HttpClient
        services.AddScoped(sp => new HttpClient 
        { 
            BaseAddress = new Uri("http://localhost:5146"),
            Timeout = TimeSpan.FromSeconds(30)
        });

        // Add Blazored LocalStorage
        services.AddBlazoredLocalStorage();

        // Register Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IProductService, ProductService>();

        // Add Blazor WebView
        services.AddWindowsFormsBlazorWebView();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif

        var serviceProvider = services.BuildServiceProvider();
        var mainForm = new MainForm(serviceProvider);
        System.Windows.Forms.Application.Run(mainForm);
    }
}
