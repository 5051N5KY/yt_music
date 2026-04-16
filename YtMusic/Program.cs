namespace YtMusic;

static class Program
{
    [STAThread]
    static void Main()
    {
        // Configures DPI awareness, visual styles and text rendering
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
