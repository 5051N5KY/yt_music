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
| Visual Studio (build) | Visual Studio 2022 (Community or higher) |

> **Note:** WebView2 Runtime is **already installed** on every machine running Windows 11 or Microsoft Edge. No separate installation is needed on typical machines.

---

## How to build the EXE in Visual Studio

### 1. Open the project

1. Open **Visual Studio 2022**
2. Click **File → Open → Project/Solution**
3. Select the `YtMusic.sln` file

### 2. Restore NuGet packages

Visual Studio does this automatically on first open.  
You can also right-click the project → **Restore NuGet Packages**.

Required NuGet package:
- `Microsoft.Web.WebView2` (version `1.0.2849.39` or newer)

### 3. Build Debug version (for testing)

1. Set configuration to **Debug** (top toolbar)
2. Press **F5** or click **Debug → Start Debugging**
3. The app should launch and load `https://music.youtube.com`

### 4. Build Release version (for distribution)

**Option A – standard build (requires .NET Runtime on the target machine):**

1. Switch configuration to **Release**
2. Click **Build → Build Solution** (Ctrl+Shift+B)
3. EXE will be in: `YtMusic\bin\Release\net8.0-windows\YtMusic.exe`

**Option B – single EXE (publish, recommended):**

1. Right-click the `YtMusic` project → **Publish...**
2. Select the `win-x64` profile (or create a new one: Folder → Next → Finish)
3. Click **Publish**
4. EXE will be in: `publish\YtMusic.exe`

**Option C – from the command line:**

```bash
cd YtMusic
dotnet publish -p:PublishProfile=win-x64
```

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
