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
    ├── MainForm.cs                      # Main window with WebView2
    ├── WindowSettings.cs                # Save/restore window size and position
    └── Properties/
        └── PublishProfiles/
            └── win-x64.pubxml           # Publish profile: single EXE (x64)
```

---

## Requirements

| Component | Requirement |
|---|---|
| OS | Windows 10 / 11 (64-bit) |
| .NET Runtime | [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |
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
dotnet publish -c Release -r win-x64 --self-contained
```

EXE will be in: `YtMusic\bin\Release\net8.0-windows\win-x64\publish\YtMusic.exe`

---

## How to get a fully self-contained EXE (no .NET Runtime required)

In `YtMusic/Properties/PublishProfiles/win-x64.pubxml` change:

```xml
<SelfContained>false</SelfContained>
```
to:
```xml
<SelfContained>true</SelfContained>
```

Then publish again. The EXE will be larger (~150 MB) but does not require .NET Runtime to be installed.

---

## How the app works

- On launch, a window opens with `https://music.youtube.com` loaded
- **User session** (login, cookies, localStorage) is preserved between launches  
  (data stored in `%APPDATA%\YtMusic\WebView2\`)
- **Window size and position** are remembered after closing  
  (saved in `%APPDATA%\YtMusic\settings.json`)
- Default size: **1200×800**, minimum: **900×600**
- Supports: closing, minimizing, maximizing, and resizing
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
