using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace YtMusic;

public class MainForm : Form
{
    private readonly WebView2       _webView;
    private readonly WindowSettings _settings;
    private readonly NotifyIcon     _trayIcon;
    private Button? _btnMax;

    [DllImport("user32.dll")] private static extern bool   ReleaseCapture();
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HT_CAPTION       = 0x2;
    private const int WM_NCCALCSIZE    = 0x83;
    private const int WM_NCHITTEST     = 0x84;
    private const int WM_NCLBUTTONUP   = 0xA2;
    private const int HTMAXBUTTON      = 9;
    private const int HTLEFT           = 10;
    private const int HTRIGHT          = 11;
    private const int HTTOP            = 12;
    private const int HTTOPLEFT        = 13;
    private const int HTTOPRIGHT       = 14;
    private const int HTBOTTOM         = 15;
    private const int HTBOTTOMLEFT     = 16;
    private const int HTBOTTOMRIGHT    = 17;
    private const int ResizeBorder     = 6;

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

        _btnMax = MakeTitleButton("", accentColor, hoverAccent);
        _btnMax.Click += (_, _) =>
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal : FormWindowState.Maximized;
            _btnMax.Text = WindowState == FormWindowState.Maximized ? "" : "";
        };

        var btnMin = MakeTitleButton("", accentColor, hoverAccent);
        btnMin.Click += (_, _) => WindowState = FormWindowState.Minimized;

        // lblTitle (Fill) added first; buttons added right-to-left:
        // first added among Right-docked = rightmost position
        titleBar.Controls.Add(lblTitle);
        titleBar.Controls.Add(btnMin);
        titleBar.Controls.Add(_btnMax);
        titleBar.Controls.Add(btnClose);
        btnClose.Dock = DockStyle.Right;
        _btnMax.Dock  = DockStyle.Right;
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

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.Style |= 0x00C00000 | 0x00040000; // WS_CAPTION | WS_THICKFRAME - enables Snap
            return cp;
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_NCCALCSIZE && m.WParam != IntPtr.Zero)
        {
            m.Result = IntPtr.Zero;
            return;
        }

        if (m.Msg == WM_NCLBUTTONUP && m.WParam.ToInt32() == HTMAXBUTTON)
        {
            ToggleMaximize();
            return;
        }

        base.WndProc(ref m);

        if (m.Msg == WM_NCHITTEST)
            HandleHitTest(ref m);
    }

    private void HandleHitTest(ref Message m)
    {
        if (_btnMax is { IsHandleCreated: true })
        {
            var pt = new Point((short)(m.LParam.ToInt32() & 0xFFFF), (short)(m.LParam.ToInt32() >> 16));
            if (_btnMax.RectangleToScreen(_btnMax.ClientRectangle).Contains(pt))
            {
                m.Result = (IntPtr)HTMAXBUTTON;
                return;
            }
        }

        if (WindowState != FormWindowState.Normal) return;

        var c    = PointToClient(Cursor.Position);
        bool l   = c.X < ResizeBorder;
        bool r   = c.X >= ClientSize.Width  - ResizeBorder;
        bool t   = c.Y < ResizeBorder;
        bool bot = c.Y >= ClientSize.Height - ResizeBorder;

        m.Result = (l, r, t, bot) switch
        {
            (true,  _,    true,  _   ) => (IntPtr)HTTOPLEFT,
            (_,     true, true,  _   ) => (IntPtr)HTTOPRIGHT,
            (true,  _,    _,     true) => (IntPtr)HTBOTTOMLEFT,
            (_,     true, _,     true) => (IntPtr)HTBOTTOMRIGHT,
            (true,  _,    _,     _   ) => (IntPtr)HTLEFT,
            (_,     true, _,     _   ) => (IntPtr)HTRIGHT,
            (_,     _,    true,  _   ) => (IntPtr)HTTOP,
            (_,     _,    _,     true) => (IntPtr)HTBOTTOM,
            _                          => m.Result
        };
    }

    private void ToggleMaximize()
    {
        WindowState = WindowState == FormWindowState.Maximized
            ? FormWindowState.Normal
            : FormWindowState.Maximized;
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
                return Color.FromArgb(255, (byte)(argb & 0xFF), (byte)(argb >> 8 & 0xFF), (byte)(argb >> 16 & 0xFF));
        }
        catch { }
        return Color.FromArgb(26, 26, 26);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (_btnMax != null)
            _btnMax.Text = WindowState == FormWindowState.Maximized ? "" : "";
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
        if (_settings.IsMaximized)
            WindowState = FormWindowState.Maximized;

        try
        {
            var env = await CoreWebView2Environment.CreateAsync(null, UserDataFolder);
            await _webView.EnsureCoreWebView2Async(env);
            _webView.Source = new Uri("https://music.youtube.com");
        }
        catch (WebView2RuntimeNotFoundException)
        {
            MessageBox.Show(
                "Microsoft WebView2 Runtime was not found.\n\n" +
                "Download it from:\nhttps://developer.microsoft.com/microsoft-edge/webview2/\n\n" +
                "After installation, restart the application.",
                "YouTube Music – WebView2 missing",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            Application.Exit();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error during startup:\n{ex.Message}",
                "YouTube Music", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        _trayIcon.Visible = false;

        if (WindowState == FormWindowState.Normal)
        {
            _settings.X      = Location.X;
            _settings.Y      = Location.Y;
            _settings.Width  = Size.Width;
            _settings.Height = Size.Height;
        }
        _settings.IsMaximized = WindowState == FormWindowState.Maximized;
        _settings.Save();
    }
}
