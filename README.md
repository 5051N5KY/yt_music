# YouTube Music Desktop

A minimalist desktop wrapper for [YouTube Music](https://music.youtube.com) on Windows.  
Built with **C# + WinForms + Microsoft WebView2**. A lightweight alternative to Electron.

---

## Project structure

```
yt_music/
├── YtMusic.sln                          # Visual Studio solution file
└── YtMusic/
    ├── YtMusic.csproj                   # Project file (.NET 8, WinForms)
    ├── Program.cs                       # Application entry point
    ├── MainForm.cs                      # Main window with WebView2 + custom title bar
    ├── WindowSettings.cs                # Save/restore window size and position
    ├── youtube.ico                      # Application icon
    └── Properties/
        └── PublishProfiles/
            └── win-x64.pubxml           # Publish profile: single self-contained EXE (x64)
```

---

## Requirements

| Component | Requirement |
|---|---|
| OS | Windows 10 / 11 (64-bit) |
| .NET Runtime | Only needed for Debug/development. The published EXE is self-contained — no runtime required. |
| WebView2 Runtime | Built into Windows 11 / Edge; [download here](https://developer.microsoft.com/microsoft-edge/webview2/) |
| Editor (optional) | [Visual Studio Code](https://code.visualstudio.com/) with the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) extension |

> **Note:** WebView2 Runtime is **already installed** on every machine running Windows 11 or Microsoft Edge. No separate installation is needed on typical machines.

---

## How to build the EXE

### Using Visual Studio Code

1. Install [Visual Studio Code](https://code.visualstudio.com/) and the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) extension
2. Open the `yt_music` folder in VS Code
3. NuGet packages restore automatically (or run `dotnet restore` in the terminal)

**Run (debug):**

Press **F5** — VS Code will build and launch the app.

**Build Release:**

```bash
dotnet build -c Release
```

EXE will be in: `YtMusic\bin\Release\net8.0-windows\YtMusic.exe`

**Publish — single self-contained EXE (recommended):**

```bash
dotnet publish YtMusic/YtMusic.csproj -p:PublishProfile=win-x64
```

EXE will be in: `C:\Users\<you>\publish\YtMusic.exe` — no .NET Runtime required on the target machine.

---

## How the app works

- On launch, a window opens with `https://music.youtube.com` loaded
- **Custom title bar** in Windows accent color with minimize / maximize / close buttons
- **System tray icon** — right-click for menu: Przywróć / Minimalizuj / Dark Mode / Light Mode / Zamknij
- **User session** (login, cookies, localStorage) is preserved between launches  
  (data stored in `%APPDATA%\YtMusic\WebView2\`)
- **Window size and position** are remembered after closing  
  (saved in `%APPDATA%\YtMusic\settings.json`)
- Default size: **1200×800**, minimum: **900×600**
- Supports resizing from all edges and corners
- If WebView2 Runtime is not installed, a clear message with instructions is shown

---

## Why WinForms + WebView2?

| Technology | Size | Complexity | Startup speed |
|---|---|---|---|
| **WebView2 + WinForms** (this solution) | ~5 MB EXE | Minimal | Fast |
| Electron | ~150 MB | High | Slow |
| CEFSharp + WinForms | ~100 MB | Medium | Medium |

- **WinForms** is simpler than WPF for this use case (single window, single control)
- **WebView2** uses the system Edge/Chromium engine — no need to bundle a browser
- User session works identically to Edge
- The project is clean, minimal, and easy to maintain

---

## Author

Made by **Sosinsky** — [github.com/5051N5KY/yt_music](https://github.com/5051N5KY/yt_music)

---

## Disclaimer

YouTube Music, the YouTube logo, and all related trademarks are the property of **Google LLC**.  
This project is an unofficial desktop wrapper and is **not affiliated with, endorsed by, or sponsored by Google or YouTube** in any way.  
All rights to the YouTube Music service and its content belong to their respective owners.
