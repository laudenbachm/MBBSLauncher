# Changelog

All notable changes to MBBS Launcher will be documented in this file.

The format for change tracking: `YY.MM.DD.X - HH:MMAM/PM`
- YY = Year (26 = 2026)
- MM = Month
- DD = Day
- X = Change number for that day (starts at 1, increments with each change)
- HH:MM = Time in 12-hour format with AM/PM

## [v1.70] - 2026-02-19

### 26.02.19.8
**App Manager Status Label Truncation Fix**

#### Fixed
- **"Launch 0:30" truncated to just "Launch" in App Manager**
  - Root cause: 110px status label + Consolas 11pt font — "Launch 0:30" wraps at the space, hiding the time
  - Fix: widened form from 280px to 310px; increased status label base width from 110 to 140px

#### Technical Details
- `Forms/AppManagerForm.Designer.cs`: `ClientSize.Width` 280→310; `MinimumSize.Width` 280→310
- `Forms/AppManagerForm.cs`: `statusW` base value 110→140 in `UpdateDisplay()`

---

### 26.02.19.7
**Fix BBS Showing "Crashed" on Clean Shutdown**

#### Fixed
- **BBS process stop always reported as "Crashed"**
  - When the BBS was stopped normally, App Manager showed "Crashed" instead of "Stopped"
  - Fix: changed stop detection to always transition to `Stopped`; removed the separate `Crashed` persistent state (was never useful — there's no way to distinguish crash from clean exit at the OS level)
  - Status text updated: `"✓ Running"` → `"Running"`, `"⨯ Stopped"` → `"Stopped"`, `"⚠ Stopped"` → `"Crashed"` simplified to `"Crashed"` for the rare edge case

#### Technical Details
- `Forms/AppManagerForm.cs`: `MonitorPrograms()` — stop detection always sets `ProgramStatus.Stopped`; removed "keep crashed status" branch; simplified else-if chain
- `Models/ManagedProgram.cs`: `GetStatusText()` — removed Unicode symbols; plain text for all statuses

---

### 26.02.19.6
**App Manager DPI-Aware Label Sizing**

#### Fixed
- **Countdown timer label clipped at 125%+ display scale**
  - Designer controls auto-scale with `AutoScaleMode.Font`; controls created in code do NOT
  - At 150% DPI, 11pt Consolas characters are wide enough that "Launch 0:30" overflows fixed-pixel labels
  - Fix: compute `DeviceDpi / 96` scale factor at runtime and apply to all row coordinates and label widths in `UpdateDisplay()`
  - Status labels changed from `BackColor = Transparent` to solid `Color.FromArgb(0, 0, 128)` — transparent background caused stale paint artifacts when text changed length

#### Technical Details
- `Forms/AppManagerForm.cs`: `UpdateDisplay()` — all row/label dimensions multiplied by `sf = DeviceDpi / 96f`; font size 9pt→11pt; `BackColor` on name/status labels set to solid navy; `TextAlign` on status changed to `MiddleLeft`

---

### 26.02.19.5
**App Manager Resizable Form**

#### Added
- **Resizable App Manager window (bottom-edge drag)**
  - App Manager can now be resized vertically by dragging the bottom edge
  - Height is persisted to `[AppManager] LastHeight` in `MBBSLauncher.ini` and restored on next launch
  - Program list panel anchored to all four sides so it stretches with the form
  - Cancel button moves inside the scrollable panel, below the last program row
  - `MinimumSize` enforced at 310×220 to prevent layout collapse

#### Technical Details
- `Forms/AppManagerForm.Designer.cs`: `Anchor` added to all controls; `MinimumSize = new Size(310, 220)`
- `Forms/AppManagerForm.cs`: `WndProc` override returns `HTBOTTOM` when cursor is within 6px of bottom edge; `LoadSettings()` restores `LastHeight`; `SaveSettings()` persists it; `_programListPanel` uses `this.ClientSize.Width - 20` instead of hardcoded 260px

---

### 26.02.19.3
**SaveSettings Init Guard + Red Heart Title Bar**

#### Fixed
- **App Manager writing Opacity=0 to INI on startup**
  - `SaveSettings()` was firing during `InitializeComponent` before the TrackBar value was loaded, persisting 0% opacity
  - Fix: `_initialized` flag gates all `SaveSettings()` calls; flag is set after `LoadSettings()` completes

#### Added
- **Red heart (♥) in Windows title bar**
  - Painted via `WM_NCPAINT` override in `MainForm`

#### Technical Details
- `Forms/AppManagerForm.cs`: `_initialized` bool field; guard added to `SaveSettings()`
- `Forms/MainForm.cs`: `WM_NCPAINT` handler draws red heart glyph in non-client title area

---

### 26.02.19.2
**Bug Fixes and App Manager Opacity Control**

#### Fixed
- **Paint error on close when App Manager is open**
  - `ArgumentException: Parameter is not valid` no longer thrown in `MainForm_Paint` during shutdown
  - Root cause: `Image.FromStream` keeps a reference to the source stream; the `using` block was disposing the stream immediately after load, leaving the image's internal stream pointer dangling
  - Fix: load image into a `Bitmap` copy via `new Bitmap(temp)` — the Bitmap decodes all pixel data upfront and has no runtime dependency on the original stream
- **Auto-Launch countdown time not visible in App Manager**
  - Status label text "⏱ Launch: 1:30" was being clipped; the ⏱ clock emoji is not in Consolas and triggers a wider fallback glyph, pushing the text past the 100px label width
  - Fix: changed format to `"Launch 0:30"` — no emoji, shorter text, fits the status label

#### Added
- **App Manager opacity/transparency slider**
  - New "Opacity:" TrackBar in MBBS App Manager between the program list and checkboxes
  - Range: 20%–100% in steps of 5 (keyboard) / 10 (page keys)
  - Current value shown as cyan percentage label (e.g., "75%")
  - Persisted to `[AppManager] Opacity` in `MBBSLauncher.ini`
  - Restored on next launch; defaults to 60% for new installs

#### Technical Details
- `MainForm.cs`: `LoadBackgroundImage()` — `Image.FromStream` replaced with `new Bitmap(Image.FromStream(stream))`
- `Models/ManagedProgram.cs`: `GetStatusText()` Pending case: `"⏱ Launch: M:SS"` → `"Launch M:SS"`
- `Forms/AppManagerForm.Designer.cs`: Added `TrackBar opacityTrackBar`, `Label opacityLabel`, `Label opacityValueLabel`; checkboxes moved from y=210 to y=212; form height 235→240; `BeginInit`/`EndInit` wrapping for TrackBar
- `Forms/AppManagerForm.cs`: `LoadSettings()` reads and applies opacity; `SaveSettings()` persists it; `OpacityTrackBar_Scroll` handler updates label and `this.Opacity`
- `MBBSLauncher.csproj`: Version updated to 1.70.0.0

---

## [v1.60] - 2026-02-19

### 26.02.19.1
**UI Refinements, Messaging Clarity, and Improved Defaults**

#### Added
- **Support section in Advanced tab**
  - New "Support" section at the bottom of the Advanced configuration tab
  - Clickable link to `https://github.com/laudenbachm/MBBS-Launcher/discussions`
  - Opens in default browser via `Process.Start` with `UseShellExecute = true`

#### Changed
- **Automatic version display in F1 Help popup**
  - Version header in Help content now reads from `Program.APP_VERSION` at runtime
  - No manual version string edits required in `HelpForm.cs` for future releases
- **Neutral BBS stop messaging**
  - System tray balloon title changed from "BBS Crashed" to "BBS has stopped."
  - Balloon body text: removed "unexpectedly" — now reads "The Major BBS has stopped. Main window restored."
  - App Manager status display: "⚠ CRASHED" replaced with "⚠ Stopped"
  - Applies across tray notifications and MBBS App Manager status column
- **MBBS App Manager layout fix**
  - Program list now starts with one blank line of top padding (22px) to prevent the "The Major BBS" entry from appearing visually flush with or clipped by the title bar
- **Auto-Launch tab column widths**
  - All columns are visible by default at standard window size — no horizontal scroll required
  - FillWeight-based proportional sizing (total = 100): Enabled 8%, Name 20%, Path 37%, Arguments 15%, Delay (s) 12%, Min. 8%
  - `MinimumWidth` set on all columns to maintain usability when window is resized
- **Auto-Start tab text no longer crops**
  - Info label height increased from 35px to 45px — both lines fully visible at 100% and 125% DPI
- **Advanced tab cleanup**
  - Removed duplicate GitHub URL from "About" section (already shown in top header of config window)
- **Improved default configuration for new installs**
  - "Launch MBBS Launcher automatically at Windows start" — enabled by default
  - "Show Icon in System Tray" — enabled by default (was already true, now explicitly documented)
  - Config editor checkbox reflects INI default for new installs where registry entry not yet written
  - Existing user configurations are not affected

#### Technical Details
- `HelpForm.cs`: `@"..."` literal changed to `$@"..."` interpolated verbatim string
- `ManagedProgram.cs`: `ProgramStatus.Crashed` display text updated; enum comment updated
- `AppManagerForm.cs`: `UpdateDisplay()` starts `yPos` at 22 instead of 0
- `ConfigEditorForm.cs`: `CreateAutoStartTab()` label height 35→45; `CreateAutoLaunchTab()` column FillWeights/MinimumWidths; `CreateAdvancedTab()` GitHub URL removed, Support section added; `LoadConfiguration()` checks INI as fallback for startup checkbox
- `ConfigManager.cs`: `CreateDefaultConfig()` sets `AutoLaunchAtStartup=true`
- `MBBSLauncher.csproj`: Version updated to 1.60.0.0

---

## [v1.55] - 2026-02-18

### 26.02.18.1 - 12:00PM
**Auto Launch Duplicate Instance Prevention**

#### Added
- **Process Already-Running Check for Auto Launch**
  - Auto Launch now checks whether a configured application is already running on the system before attempting to launch it
  - If a matching process is found, the launch is skipped and the event is logged to `audit.log`
  - Prevents duplicate instances of auto-launch programs (e.g., Ghost3, Telnet server) from being spawned on BBS restart or re-launch
  - Works correctly regardless of whether the program was launched by MBBS Launcher or started externally
  - Process detection uses the executable filename (without `.exe` extension) via `Process.GetProcessesByName()`

#### Technical Details
- Check added to `AutoLaunchManager.LaunchProgram()` immediately after the file-exists guard
- Uses existing `ProcessHelper.IsProcessRunning()` method — no new dependencies
- Skipped programs still fire the `ProgramLaunched` event (`Success = true, ErrorMessage = "Already running - skipped"`) so the countdown UI clears correctly
- Invalid or empty process names are handled gracefully (launch proceeds normally)
- Manual menu launches (Options 1–8) are **not** affected — check is auto-launch only

---

## [v1.6] - 2026-02-11

### 26.02.11.1 - 09:00PM
**Administrator Privileges Required**

#### Added
- **Run as Administrator**
  - Application now requires administrator privileges for proper operation
  - UAC elevation prompt automatically displayed on launch
  - Ensures proper process management and system integration on Windows 11
  - Application manifest (app.manifest) with requireAdministrator execution level
  - Enhanced compatibility declarations for Windows 7-11
  - DPI awareness settings for better display on high-DPI screens

#### Changed
- Updated version to 1.6.0.0 in all file properties
- README updated with administrator requirement information
- System Requirements section now mentions UAC elevation
- Installation instructions include UAC prompt step

#### Technical Details
- Added app.manifest with requireAdministrator execution level
- Manifest includes Windows compatibility declarations (Windows 7-11)
- DPI awareness set to PerMonitorV2 for optimal scaling
- ApplicationManifest property added to MBBSLauncher.csproj
- All version strings updated to 1.6.0

#### Why This Change?
A user reported issues running the launcher on Windows 11 without administrator privileges. This change ensures the launcher has the necessary permissions to:
- Properly enumerate and manage BBS processes
- Bring running applications to foreground
- Integrate with Windows startup functionality
- Access system-level APIs for process management

---

## [v1.20] - 2026-01-23

### 26.01.23.1 - 01:30PM
**Ghost3 Support & UI Improvements**

#### Added
- **Ghost3 Auto-Launch Support**
  - Auto-launch Ghost3 after The Major BBS starts (configurable delay)
  - Ghost3 path configuration with browse dialog
  - Countdown timer (0-300 seconds, default 60) with visual feedback
  - Green-themed countdown display (distinguishable from BBS countdown)
  - Press any key or click to cancel Ghost3 launch
  - Independent operation - no process monitoring required
- **Updated Background Image**
  - New launcher screen with updated bottom text
  - "Run The Major BBS: begin taking calls and serving users."
- **Enhanced Help Dialog (F1)**
  - Increased font size from 10pt to 11pt for better readability
  - Disabled word wrapping for proper text formatting
  - Wider window (820px from 700px) to accommodate larger font
  - Added comprehensive Ghost3 documentation section
  - Updated to reflect v1.20 features
- **Configuration Window Improvements**
  - Now fully resizable (was fixed dialog)
  - Added maximize button for flexibility
  - Minimum size set to 650x600px for low-resolution displays
  - Works perfectly on 1024x768 monitors
  - Clean header layout with version info and author credit
  - Save/Cancel buttons repositioned for better visibility

#### Changed
- Ghost3 settings layout matches BBS auto-start style
- All checkboxes aligned consistently at x=40 margin
- Configuration editor header redesigned for cleaner appearance
- Form layouts optimized for various screen resolutions

#### Fixed
- Configuration editor Save/Cancel button visibility issues
- Button positioning in resizable configuration window
- Form resize behavior improvements
- Checkbox alignment consistency

#### Technical Details
- Updated version to 1.20.0.0 in all file properties
- Ghost3 settings added to ConfigManager.cs default configuration
- New configuration options:
  - `Ghost3Enabled` (default: false)
  - `Ghost3Path` (default: C:\Ghost3\Ghost3.exe)
  - `Ghost3Delay` (default: 60 seconds)
- Ghost3 countdown timer integrated into MainForm.cs
- Help content expanded with Ghost3 usage instructions
- Background image resource updated (embedded in executable)

---

## [v1.10] - 2026-01-13

### 26.01.13.1 - 12:00PM
**Version Info Fix & Feature Update**

#### Fixed
- Updated version information in file properties to correctly display v1.10.0.0
- Previously, Windows file properties showed v1.0.0.0 instead of the actual version

#### Added (from v1.10 development)
- System tray support with minimize to tray functionality
- Auto-start with Windows option in system tray menu
- Quick access to website, demo BBS, and Discord from system tray context menu
- All links now clickable in the UI
- System tray icon with comprehensive context menu
- Minimize to tray on window minimize
- Double-click tray icon to show/hide main window

#### Technical Details
- Updated AssemblyVersion to 1.10.0.0
- Updated FileVersion to 1.10.0.0
- Updated ProductVersion to 1.10.0
- Maintenance release focused on version display accuracy

---

## [v1.00] - 2026-01-07

### 26.01.07.1 - 06:00PM
**Initial Release**

#### Added
- Classic DOS-style retro interface inspired by Major BBS v6.25 launcher
- Background image support with 16:9 scalable window
- 9 configurable menu options (1-9) plus Exit (0)
- Keyboard input support (number keys 0-9)
- F12 configuration editor
- INI-based configuration system (`MBBSLauncher.ini`)
- Process detection for running programs
- WGServer detection to prevent conflicts
- Auto-hide/show functionality when launching programs
- Bring-to-foreground capability for already running applications
- Automatic search for BBSV10 and WGSERV folders on first run
- Configuration editor with:
  - Path configuration for BBSV10 and WGSERV
  - Program path configuration for all 9 menu options
  - Custom naming for menu options
  - Browse dialogs for folder and file selection
- Special handling for wgsappgo.exe → wgserver.exe process chain
- Version information embedded in executable
- GitHub repository integration
- MIT License
- Comprehensive README and documentation

#### Technical Details
- Built with .NET 6.0 WinForms
- 32-bit (x86) executable
- Single-file publish support
- Compatible with Windows 7+, Windows Server 2012+
- File change tracking implemented in all source files

#### File Structure
```
MBBSLauncher/
├── src/
│   └── MBBSLauncher/
│       ├── Forms/
│       │   ├── MainForm.cs
│       │   ├── MainForm.Designer.cs
│       │   ├── ConfigEditorForm.cs
│       │   └── ConfigEditorForm.Designer.cs
│       ├── Resources/
│       │   ├── background.png
│       │   └── icon.png
│       ├── Program.cs
│       ├── ConfigManager.cs
│       ├── ProcessHelper.cs
│       └── MBBSLauncher.csproj
├── images/
│   ├── MBBS Launcher Screen.png
│   └── MBBS Launcher ICON 1024x1024.png
├── docs/
├── README.md
├── CHANGELOG.md
├── LICENSE
└── .gitignore
```

#### Known Limitations
- Icon file needs conversion from PNG to ICO format (requires Windows tools)
- Mouse selection with arrow keys planned for future version
- Single executable requires .NET 6.0 Desktop Runtime on target system

---

## Version Numbering Scheme

MBBS Launcher uses the following versioning scheme:
- **Major.Minor** format (e.g., v1.00)
- Major version increments for significant feature additions or breaking changes
- Minor version increments for bug fixes and small features
- Individual file changes tracked in source code headers using YY.MM.DD.X format

---

**Note:** This changelog follows the [Keep a Changelog](https://keepachangelog.com/) format and uses [Semantic Versioning](https://semver.org/) principles adapted for the project's versioning scheme.
