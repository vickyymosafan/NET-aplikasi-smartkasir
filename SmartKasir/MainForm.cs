using Microsoft.AspNetCore.Components.WebView.WindowsForms;

namespace SmartKasir.Client;

public partial class MainForm : Form
{
    private BlazorWebView blazorWebView;

    public MainForm(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        
        Text = "SmartKasir POS";
        Size = new Size(1366, 768);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(9, 9, 11);

        // Create BlazorWebView
        blazorWebView = new BlazorWebView
        {
            Dock = DockStyle.Fill,
            HostPage = "wwwroot/index.html"
        };

        blazorWebView.RootComponents.Add<App>("#app");
        blazorWebView.Services = serviceProvider;

        Controls.Add(blazorWebView);
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        ResumeLayout(false);
    }
}
