# MBBS Launcher

**Version:** v1.70

## Screenshot

![MBBS Launcher Screenshot](images/1280x720%20SS.gif)
**Created by:** Mark Laudenbach
**Created with Love in:** Iowa
**License:** MIT

## About

MBBS Launcher is a Windows application that provides easy access to tools and utilities for The Major BBS Version 10 sysops. Inspired by the classic DOS-era Major BBS launcher interface, this modern version brings the nostalgic feel of the original while adding contemporary features and usability.

**📺 Watch the v1.55 Demo Video:** https://www.youtube.com/watch?v=RJnNRqCFjWs 

## 🐛 Bug Disclosure

This was coded by a guy who Googles "how to exit vim" every single time. There WILL be bugs. There WILL be spelling mistakes. There WILL be profanity shouted at my monitor. You've been warned!

## Features

### Core Features
- **Retro DOS-Style Interface** - Classic blue screen design reminiscent of the original Major BBS v6.25 launcher
- **Easy Program Access** - Launch BBS utilities and tools with keyboard (0-9, 99) or mouse clicks
- **Clickable Menu Options** - Click any menu button with your mouse or use traditional keyboard input
- **Configurable Menu** - Customize program paths and menu options via F12 configuration editor
- **Smart Process Management** - Automatically detects if programs are already running and brings them to foreground
- **WGServer Protection** - Prevents conflicts by detecting if the BBS server is already running
- **Auto-Hide/Show** - Launcher hides when programs run and reappears when they close
- **16:9 Aspect Ratio** - Modern scalable window design while maintaining the classic look
- **INI Configuration** - Simple text-based configuration file for easy editing

### v1.5 New Features
- **🎯 Self-Contained Deployment** - No .NET runtime installation required!
- **🚫 Single Instance Enforcement** - Prevents multiple launcher instances from running
- **📑 5-Tab Configuration Window** - Clean, organized settings interface
- **🚀 Multiple Auto-Launch Programs** - Launch unlimited programs after BBS starts with independent timers
- **🔽 Launch Minimized** - Programs can start minimized to prevent focus stealing
- **⬆️ Automatic Config Migration** - Seamlessly upgrades v1.20 configurations
- **📝 Audit Log with Rotation** - Diagnostic logs that auto-rotate at 500 KB
- **🎨 System Tray Integration** - Minimize to tray with context menu

## System Requirements

- **Operating System:** Windows 7 or later, Windows Server 2012 or later
- **Architecture:** 32-bit (x86) - runs on both 32-bit and 64-bit Windows
- **.NET Runtime:** **None required!** (Self-contained build)
- **Disk Space:** ~65 MB
- **Permissions:** **Administrator privileges required** - The launcher will prompt for UAC elevation on startup

### 📦 About the File Size

**Why is v1.5 larger than v1.20?**

v1.5 is a **self-contained build** that includes the entire .NET 8.0 runtime embedded in the executable. This means:

- ✅ **Zero dependencies** - No .NET runtime installation required
- ✅ **Works on any Windows PC** - Even systems without .NET installed
- ✅ **Simpler installation** - Just download and run
- ✅ **Better compatibility** - Guaranteed to work regardless of system configuration

**File size comparison:**
- **v1.20:** 1.3 MB + 150 MB .NET 6.0 runtime (required separately) = **151 MB total**
- **v1.5:** 65 MB (everything included) = **65 MB total**

Even though the single file is larger, you're actually using **less disk space overall** and eliminating the need to install .NET separately!

## Installation

1. Download the latest release from the [Releases](https://github.com/laudenbachm/MBBS-Launcher/releases) page
2. Place `MBBSLauncher.exe` in your BBS directory (or any folder of your choice)
3. Run `MBBSLauncher.exe`
4. Click **Yes** when Windows asks for administrator permission (UAC prompt)

**That's it!** No .NET runtime installation needed. All dependencies are included in the single executable.

> **Upgrading from v1.60 or earlier?** The executable was renamed from `MBBS Launcher.exe` to `MBBSLauncher.exe` in v1.70. You can delete the old file — your `MBBSLauncher.ini` settings are fully preserved.

### Why Administrator Privileges?

MBBS Launcher requires administrator privileges to properly manage BBS processes and system integration features. When you launch the application, Windows will display a User Account Control (UAC) prompt asking for permission. This is normal and required for the launcher to function correctly on Windows 11 and other modern Windows versions.

## Antivirus False Positives

Some antivirus software may flag MBBS Launcher as suspicious due to legitimate behaviors that are common in system utilities:

- **Process enumeration** - Checking if BBS programs are already running
- **Window manipulation** - Bringing running programs to foreground
- **Launching executables** - Starting BBS utilities on your behalf
- **Startup integration** - Optional auto-launch at Windows startup

**This is a FALSE POSITIVE.** The application is completely safe and open source.

### What We're Doing About It

- The executable has been submitted to Microsoft Defender and other major AV vendors for whitelisting
- All source code is publicly available in this repository for review
- We are working on obtaining a code signing certificate to eliminate these warnings

### If Windows Defender Blocks or Deletes the File

**Windows Defender may quarantine this file.** To use MBBS Launcher, you need to add an exclusion:

1. Open **Windows Security** (search for it in Start menu)
2. Go to **Virus & threat protection** → **Manage settings**
3. Scroll down to **Exclusions** → Click **Add or remove exclusions**
4. Click **Add an exclusion** → **Folder**
5. Browse to and select the folder containing `MBBSLauncher.exe`
6. Click **Select Folder**

**Note:** This is safe to do because the application is open source and has been verified clean (VirusTotal: 2/70 detection rate, no major AV vendors flagged it).

### If Other Antivirus Software Flags It

1. **Review the source code** - All code is available in this repository
2. **Scan on VirusTotal** - Check the analysis: https://www.virustotal.com/gui/file/df63ec5c68374a8fcd753466d985d756072d0c9bb04542e7ce918a8f7fa30994
3. **Add an exception** - Add `MBBSLauncher.exe` to your antivirus exclusions
4. **Report as false positive** - Help us by reporting it to your AV vendor

For more information, see our [Security Policy](https://github.com/laudenbachm/MBBS-Launcher/security/policy).

## Usage

### Launching Programs

- **Number Keys (0-9, 99):** Press any number key to launch the corresponding program
  - Option 0: Exit the launcher
  - Options 1-8: Launch configured programs
  - Option 99: Special configuration option (CNF 99)
- **Mouse Click:** Click any menu button to launch that option
- **ESC Key:** Exit the launcher
- **F12 Key:** Open configuration editor

### First Time Setup

On first launch, the application will:
1. Search for BBSV10 and WGSERV folders on your system
2. Create a default configuration file `MBBSLauncher.ini`
3. Prompt you to configure program paths if not found automatically

### Configuration

Press **F12** to open the configuration editor where you can:
- Enable/disable auto-launch at Windows startup
- Set BBS installation path
- Configure launcher options (1-8, 99) with custom program names and paths
- Browse for executable files

Configuration is saved to `MBBSLauncher.ini` in the same directory as the launcher.

### Default Menu Layout

Based on the classic Major BBS launcher:

1. Hardware Setup
2. Design Menu Tree
3. Security & Accounting
4. General Configuration
5. **Go!** - Launch the BBS (wgsappgo.exe)
6. Edit Text Blocks
7. Offline Utilities
8. Reports
99. CNF 99
0. Exit MBBS Launcher

## Building from Source

### Prerequisites

- .NET 8.0 SDK or later
- Windows 10/11 or Windows Server 2016+ (for building)
- Visual Studio 2022 (optional, recommended)

### Build Instructions

#### Using Command Line

```bash
# Clone the repository
git clone https://github.com/laudenbachm/MBBS-Launcher.git
cd MBBS-Launcher

# Build the project
cd src/MBBSLauncher
dotnet restore
dotnet build -c Release

# Publish as self-contained single-file executable
dotnet publish -c Release -r win-x86 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

#### Using Visual Studio

1. Open `MBBSLauncher.sln` in Visual Studio 2022
2. Select **Release** configuration
3. Build > Publish
4. Configure publish settings:
   - Target: win-x86
   - Deployment mode: Self-contained
   - Produce single file: Yes
5. Click Publish

The compiled executable will be in `src/MBBSLauncher/bin/Release/net8.0-windows/win-x86/publish/`

## Configuration File Format

The `MBBSLauncher.ini` file uses standard INI format:

```ini
[Paths]
BBSPath=C:\BBSV10

[Window]
X=100
Y=100
Width=960
Height=540

[Settings]
AutoLaunchAtStartup=false

[Programs]
Option1=C:\BBSV10\WGSCNF.exe -L1
Option1Name=Hardware Setup
Option2=C:\BBSV10\wgsrunmt.exe
Option2Name=Design Menu Tree
...
Option5=C:\BBSV10\wgsappgo.exe
Option5Name=Go!
...
Option8=C:\BBSV10\WGSRPT.exe
Option8Name=Reports
Option99=C:\BBSV10\WGSCNF.exe -L99
Option99Name=CNF 99
```

You can edit this file manually or use the built-in configuration editor (F12).

## Version History

See [CHANGELOG.md](CHANGELOG.md) for detailed version history.

### Current Version: v1.70
- **App Manager** - Resizable floating status window with opacity slider, DPI-aware layout, and live countdown display
- **MBBSLauncher.exe** - Executable renamed from `MBBS Launcher.exe` to avoid GitHub's space→dot conversion on release downloads
- **Bug fixes** - Paint crash on close, BBS stop showing as "Crashed", countdown label truncation

### Previous Versions
#### v1.5
- **Self-contained deployment** - No .NET runtime installation required
- **Single instance enforcement** - Can't run launcher twice
- **5-tab configuration window** - Clean, organized interface
- **Multiple auto-launch programs** - Unlimited programs with independent timers
- **Launch minimized** - Programs start in background
- **Automatic v1.20 config migration** - Seamless upgrade
- **Audit log rotation** - Logs auto-rotate at 500 KB
- **Upgraded to .NET 8.0** - Latest framework with better performance

#### v1.20
- Ghost3 support with delayed auto-launch after BBS starts
- Updated background image
- Configurable Ghost3 path and delay settings

#### v1.10
- Fixed version information in file properties to correctly display v1.10.0.0
- System tray support with minimize to tray functionality
- Auto-start with Windows option
- Quick access to website, demo BBS, and Discord from system tray
- All links now clickable in the UI
- System tray icon with context menu

#### v1.00 - Initial Release
- Initial release
- Classic retro DOS-style interface with authentic MBBS look
- Configurable program launcher (Options 1-8, 99, and 0 for exit)
- Clickable menu buttons plus traditional keyboard input
- Auto-launch at Windows startup (optional)
- Smart process detection and management
- Auto-hide/show functionality when launching programs
- Single-file executable with embedded resources
- INI-based configuration with automatic BBS folder detection
- Built-in configuration editor (F12)
- Custom Windows application icon
- Window position and size memory
- 16:9 aspect ratio with scalable interface

## Credits

- **Created by:** Mark Laudenbach
- **Inspired by:** The Major BBS v6.25 DOS Launcher by Galacticomm, Inc.
- **For:** The Major BBS and Worldgroup sysop community

## Support

For issues, questions, or suggestions:
- Open an issue on [GitHub Issues](https://github.com/laudenbachm/MBBS-Launcher/issues)
- Check the [Documentation](docs/)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Dedication

This application was created with love for The Major BBS community. The Major BBS holds a special place in the history of online communication, and this launcher is a tribute to those great days while bringing convenience to modern sysops.

---

**MBBS Launcher v1.70** | Created with Love in Iowa | © 2026 Mark Laudenbach
