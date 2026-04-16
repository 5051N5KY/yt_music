# YouTube Music Desktop

Minimalistyczny wrapper desktopowy dla [YouTube Music](https://music.youtube.com) na Windows.  
Zbudowany w **C# + WinForms + Microsoft WebView2**. Lekka alternatywa dla Electrona.

---

## Struktura projektu

```
yt_music/
├── YtMusic.sln                          # Plik rozwiązania Visual Studio
└── YtMusic/
    ├── YtMusic.csproj                   # Plik projektu (.NET 8, WinForms)
    ├── Program.cs                       # Punkt wejścia aplikacji
    ├── MainForm.cs                      # Główne okno z WebView2
    ├── WindowSettings.cs                # Zapis/odczyt rozmiaru i pozycji okna
    └── Properties/
        └── PublishProfiles/
            └── win-x64.pubxml           # Profil publikacji: pojedynczy EXE (x64)
```

---

## Wymagania

| Składnik | Wymaganie |
|---|---|
| System | Windows 10 / 11 (64-bit) |
| .NET Runtime | [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |
| WebView2 Runtime | Wbudowany w Windows 11 / Edge; [pobierz tutaj](https://developer.microsoft.com/microsoft-edge/webview2/) |
| Visual Studio (build) | Visual Studio 2022 (Community lub wyżej) |

> **Uwaga:** WebView2 Runtime jest **już zainstalowany** na każdym komputerze z Windows 11 lub Microsoft Edge. Nie trzeba go osobno instalować na typowych maszynach.

---

## Jak zbudować EXE w Visual Studio

### 1. Otwórz projekt

1. Otwórz **Visual Studio 2022**
2. Kliknij **File → Open → Project/Solution**
3. Wybierz plik `YtMusic.sln`

### 2. Przywróć pakiety NuGet

Visual Studio zrobi to automatycznie przy pierwszym otwarciu.  
Możesz też kliknąć prawym przyciskiem na projekt → **Restore NuGet Packages**.

Wymagany pakiet NuGet:
- `Microsoft.Web.WebView2` (wersja `1.0.2849.39` lub nowsza)

### 3. Zbuduj wersję Debug (testowa)

1. Ustaw konfigurację na **Debug** (górny pasek)
2. Naciśnij **F5** lub kliknij **Debug → Start Debugging**
3. Aplikacja powinna się uruchomić i załadować `https://music.youtube.com`

### 4. Zbuduj wersję Release (do dystrybucji)

**Opcja A – standardowy build (wymaga .NET Runtime na docelowej maszynie):**

1. Zmień konfigurację na **Release**
2. Kliknij **Build → Build Solution** (Ctrl+Shift+B)
3. EXE znajdziesz w: `YtMusic\bin\Release\net8.0-windows\YtMusic.exe`

**Opcja B – pojedynczy EXE (publish, zalecane):**

1. Kliknij prawym przyciskiem na projekt `YtMusic` → **Publish...**
2. Wybierz profil `win-x64` (lub stwórz nowy: Folder → Next → Finish)
3. Kliknij **Publish**
4. EXE znajdziesz w: `publish\YtMusic.exe`

**Opcja C – z linii poleceń:**

```bash
cd YtMusic
dotnet publish -p:PublishProfile=win-x64
```

---

## Jak uzyskać w pełni samodzielny EXE (bez wymagania .NET Runtime)

W pliku `YtMusic/Properties/PublishProfiles/win-x64.pubxml` zmień:

```xml
<SelfContained>false</SelfContained>
```
na:
```xml
<SelfContained>true</SelfContained>
```

Następnie opublikuj ponownie. EXE będzie większy (~150 MB), ale nie wymaga instalacji .NET Runtime.

---

## Działanie aplikacji

- Przy uruchomieniu otwiera się okno z załadowaną stroną `https://music.youtube.com`
- **Sesja użytkownika** (logowanie, cookies, localStorage) jest zachowywana między uruchomieniami  
  (dane przechowywane w `%APPDATA%\YtMusic\WebView2\`)
- **Rozmiar i pozycja okna** są zapamiętywane po zamknięciu  
  (zapisywane w `%APPDATA%\YtMusic\settings.json`)
- Domyślny rozmiar: **1200×800**, minimalny: **900×600**
- Obsługiwane: zamykanie, minimalizacja, maksymalizacja, zmiana rozmiaru
- Jeśli WebView2 Runtime nie jest zainstalowany, wyświetlany jest czytelny komunikat z instrukcją

---

## Dlaczego WinForms + WebView2?

| Technologia | Rozmiar | Złożoność | Szybkość startu |
|---|---|---|---|
| **WebView2 + WinForms** (to rozwiązanie) | ~5 MB EXE | Minimalna | Szybki |
| Electron | ~150 MB | Wysoka | Wolny |
| CEFSharp + WinForms | ~100 MB | Średnia | Średni |

- **WinForms** jest prostszy niż WPF dla tego przypadku użycia (jedno okno, jeden control)
- **WebView2** używa systemowego silnika Edge/Chromium – brak konieczności bundlowania przeglądarki
- Sesja użytkownika działa identycznie jak w Edge
- Projekt jest czytelny, minimalny i łatwy do utrzymania
