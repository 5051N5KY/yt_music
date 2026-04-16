using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace YtMusic;

public class MainForm : Form
{
    private readonly WebView2 _webView;
    private readonly WindowSettings _settings;
    private readonly NotifyIcon _trayIcon;

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HT_CAPTION       = 2;

    // WebView2 user data folder — stores cookies, localStorage, session data.
    // This ensures the user stays logged in between launches.
    private static readonly string UserDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YtMusic", "WebView2");

    public MainForm()
    {
        _settings = WindowSettings.Load();

        _webView = new WebView2 { Dock = DockStyle.Fill };

        Text            = "YouTube Music";
        Icon            = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
        FormBorderStyle = FormBorderStyle.None;
        MinimumSize     = new Size(900, 600);
        Size            = new Size(_settings.Width, _settings.Height);

        if (_settings.X >= 0 && _settings.Y >= 0)
        {
            StartPosition = FormStartPosition.Manual;
            Location      = new Point(_settings.X, _settings.Y);
        }
        else
        {
            StartPosition = FormStartPosition.CenterScreen;
        }

        // ── Custom title bar ─────────────────────────────────────────────
        var accentColor = GetWindowsAccentColor();
        var hoverAccent = Color.FromArgb(
            Math.Min(255, accentColor.R + 45),
            Math.Min(255, accentColor.G + 45),
            Math.Min(255, accentColor.B + 45));
        var titleBar = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 32,
            BackColor = accentColor
        };

        var lblTitle = new Label
        {
            Text      = "YouTube Music",
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 9f),
            AutoSize  = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock      = DockStyle.Fill,
            Padding   = new Padding(8, 0, 0, 0)
        };

        var btnClose = MakeTitleButton("", accentColor, Color.FromArgb(232, 17, 35));
        btnClose.Click += (_, _) => Application.Exit();

        var btnMax = MakeTitleButton("", accentColor, hoverAccent);
        btnMax.Click += (_, _) =>
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal : FormWindowState.Maximized;
            btnMax.Text = WindowState == FormWindowState.Maximized ? "" : "";
        };

        var btnMin = MakeTitleButton("", accentColor, hoverAccent);
        btnMin.Click += (_, _) => WindowState = FormWindowState.Minimized;

        // lblTitle (Fill) added first; buttons added right-to-left:
        // first added among Right-docked = rightmost position
        titleBar.Controls.Add(lblTitle);
        titleBar.Controls.Add(btnMin);
        titleBar.Controls.Add(btnMax);
        titleBar.Controls.Add(btnClose);
        btnClose.Dock = DockStyle.Right;
        btnMax.Dock   = DockStyle.Right;
        btnMin.Dock   = DockStyle.Right;

        // Dragging — send WM_NCLBUTTONDOWN HT_CAPTION to let Windows handle the move
        void StartDrag(object? s, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
            }
        }
        titleBar.MouseDown += StartDrag;
        lblTitle.MouseDown += StartDrag;

        Controls.Add(_webView);
        Controls.Add(titleBar);

        // ── System tray ───────────────────────────────────────────────────
        var trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Przywróć",    null, (_, _) => RestoreWindow());
        trayMenu.Items.Add("Minimalizuj", null, (_, _) => WindowState = FormWindowState.Minimized);
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add("Dark Mode",  null, (_, _) => SetTheme("dark"));
        trayMenu.Items.Add("Light Mode", null, (_, _) => SetTheme("light"));
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add("Zamknij", null, (_, _) => Application.Exit());

        _trayIcon = new NotifyIcon
        {
            Icon             = Icon,
            Text             = "YouTube Music",
            ContextMenuStrip = trayMenu,
            Visible          = true
        };
        _trayIcon.DoubleClick += (_, _) => RestoreWindow();

        Load        += OnLoad;
        FormClosing += OnFormClosing;
    }

    // ── Resize support for borderless window ────────────────────────────
    protected override void WndProc(ref Message m)
    {
        const int WM_NCHITTEST  = 0x84;
        const int HTLEFT        = 10;
        const int HTRIGHT       = 11;
        const int HTTOP         = 12;
        const int HTTOPLEFT     = 13;
        const int HTTOPRIGHT    = 14;
        const int HTBOTTOM      = 15;
        const int HTBOTTOMLEFT  = 16;
        const int HTBOTTOMRIGHT = 17;
        const int border        = 6;

        base.WndProc(ref m);

        if (m.Msg == WM_NCHITTEST && WindowState == FormWindowState.Normal)
        {
            var c    = PointToClient(Cursor.Position);
            bool l   = c.X < border;
            bool r   = c.X >= ClientSize.Width  - border;
            bool t   = c.Y < border;
            bool bot = c.Y >= ClientSize.Height - border;

            if      (t   && l) m.Result = (IntPtr)HTTOPLEFT;
            else if (t   && r) m.Result = (IntPtr)HTTOPRIGHT;
            else if (bot && l) m.Result = (IntPtr)HTBOTTOMLEFT;
            else if (bot && r) m.Result = (IntPtr)HTBOTTOMRIGHT;
            else if (l)        m.Result = (IntPtr)HTLEFT;
            else if (r)        m.Result = (IntPtr)HTRIGHT;
            else if (t)        m.Result = (IntPtr)HTTOP;
            else if (bot)      m.Result = (IntPtr)HTBOTTOM;
        }
    }

    private static Button MakeTitleButton(string text, Color baseColor, Color hoverColor)
    {
        var btn = new Button
        {
            Text      = text,
            ForeColor = Color.White,
            BackColor = baseColor,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe MDL2 Assets", 10f),
            AutoSize  = false,
            Width     = 46,
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor    = Cursors.Hand,
            TabStop   = false,
            UseVisualStyleBackColor = false
        };
        btn.FlatAppearance.BorderSize         = 0;
        btn.FlatAppearance.MouseOverBackColor = hoverColor;
        btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(hoverColor, 0.1f);
        return btn;
    }

    private static Color GetWindowsAccentColor()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
            if (key?.GetValue("AccentColor") is int argb)
            {
                byte r = (byte)( argb        & 0xFF);
                byte g = (byte)((argb >>  8) & 0xFF);
                byte b = (byte)((argb >> 16) & 0xFF);
                return Color.FromArgb(255, r, g, b);
            }
        }
        catch { }
        return Color.FromArgb(26, 26, 26); // dark fallback
    }

    private void RestoreWindow()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private void SetTheme(string theme)
    {
        if (_webView.CoreWebView2 == null) return;
        _ = _webView.CoreWebView2.ExecuteScriptAsync(
            $"var a=document.querySelector('ytmusic-app');if(a)a.setAttribute('appearance','{theme}');");
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
        _trayIcon.Visible = false;

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
