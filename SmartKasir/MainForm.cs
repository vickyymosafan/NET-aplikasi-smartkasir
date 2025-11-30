using Microsoft.AspNetCore.Components.WebView.WindowsForms;

namespace SmartKasir.Client;

public partial class MainForm : Form
{
    private BlazorWebView? blazorWebView;

    public MainForm(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        
        Text = "SmartKasir POS";
        Size = new Size(1366, 768);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(9, 9, 11);

        try
        {
            // Get absolute path to wwwroot/index.html
            var baseDirectory = AppContext.BaseDirectory;
            var hostPagePath = Path.Combine(baseDirectory, "wwwroot", "index.html");
            
            Console.WriteLine($"[MainForm] Base Directory: {baseDirectory}");
            Console.WriteLine($"[MainForm] Host Page Path: {hostPagePath}");
            Console.WriteLine($"[MainForm] Host Page Exists: {File.Exists(hostPagePath)}");

            // Create BlazorWebView
            blazorWebView = new BlazorWebView
            {
                Dock = DockStyle.Fill,
                HostPage = hostPagePath
            };

            blazorWebView.RootComponents.Add<App>("#app");
            blazorWebView.Services = serviceProvider;

            Controls.Add(blazorWebView);
            
            Console.WriteLine("[MainForm] BlazorWebView initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MainForm] ERROR: {ex.Message}");
            Console.WriteLine($"[MainForm] Stack Trace: {ex.StackTrace}");
            MessageBox.Show($"Error initializing BlazorWebView: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        ResumeLayout(false);
    }
}
