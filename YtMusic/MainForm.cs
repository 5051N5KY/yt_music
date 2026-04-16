using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace YtMusic;

/// <summary>
/// Main application window. Hosts a WebView2 control that loads YouTube Music.
/// Window size and position are persisted across sessions in %APPDATA%\YtMusic\settings.json.
/// </summary>
public class MainForm : Form
{
    private readonly WebView2 _webView;
    private readonly WindowSettings _settings;

    // WebView2 user data folder — stores cookies, localStorage, session data.
    // This ensures the user stays logged in between launches.
    private static readonly string UserDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YtMusic",
        "WebView2");

    public MainForm()
    {
        _settings = WindowSettings.Load();

        _webView = new WebView2 { Dock = DockStyle.Fill };

        Text = "YouTube Music";
        MinimumSize = new Size(900, 600);
        Size = new Size(_settings.Width, _settings.Height);

        if (_settings.X >= 0 && _settings.Y >= 0)
        {
            StartPosition = FormStartPosition.Manual;
            Location = new Point(_settings.X, _settings.Y);
        }
        else
        {
            StartPosition = FormStartPosition.CenterScreen;
        }

        Controls.Add(_webView);

        Load += OnLoad;
        FormClosing += OnFormClosing;
    }

    private async void OnLoad(object? sender, EventArgs e)
    {
        // Restore maximized state after the window is shown to avoid positioning issues
        if (_settings.IsMaximized)
            WindowState = FormWindowState.Maximized;

        try
        {
            var env = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: UserDataFolder);

            await _webView.EnsureCoreWebView2Async(env);
            _webView.Source = new Uri("https://music.youtube.com");
        }
        catch (WebView2RuntimeNotFoundException)
        {
            ShowWebView2MissingError();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unexpected error during startup:\n{ex.Message}",
                "YouTube Music",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            Application.Exit();
        }
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        // Save size/position only when the window is in normal (non-maximized) state
        if (WindowState == FormWindowState.Normal)
        {
            _settings.X = Location.X;
            _settings.Y = Location.Y;
            _settings.Width = Size.Width;
            _settings.Height = Size.Height;
        }
        _settings.IsMaximized = WindowState == FormWindowState.Maximized;
        _settings.Save();
    }

    private static void ShowWebView2MissingError()
    {
        MessageBox.Show(
            "Microsoft WebView2 Runtime was not found.\n\n" +
            "Download and install WebView2 Runtime from:\n" +
            "https://developer.microsoft.com/microsoft-edge/webview2/\n\n" +
            "After installation, restart the application.",
            "YouTube Music – WebView2 missing",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
        Application.Exit();
    }
}
