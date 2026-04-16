using System.Text.Json;

namespace YtMusic;

/// <summary>
/// Stores and loads the main window's size, position and maximized state.
/// Settings are saved as JSON in %APPDATA%\YtMusic\settings.json.
/// </summary>
internal class WindowSettings
{
    public int X { get; set; } = -1;        // -1 means "use center screen"
    public int Y { get; set; } = -1;
    public int Width { get; set; } = 1200;
    public int Height { get; set; } = 800;
    public bool IsMaximized { get; set; } = false;

    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YtMusic",
        "settings.json");

    public static WindowSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<WindowSettings>(File.ReadAllText(FilePath))
                       ?? new WindowSettings();
        }
        catch { /* Return defaults if the file is missing or corrupted */ }
        return new WindowSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
        }
        catch { /* Silently ignore save errors */ }
    }
}
