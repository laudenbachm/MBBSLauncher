# MBBSLauncher v1.55 Release Notes

**Release Date:** February 18, 2026
**Version:** 1.55.0

## Overview

Version 1.55 delivers three significant improvements: a new App Manager window for real-time program monitoring, mandatory administrator privileges for proper Windows 10/11 operation, and automatic duplicate-instance detection for Auto Launch programs.

## What's New

### 🗂️ App Manager (Beta)

A new floating status window shows the live state of your BBS and all auto-launch programs without needing to keep the main launcher visible.

- Real-time status for each program: Running, Stopped, Pending launch, or Crashed
- Countdown display for programs that haven't launched yet
- Right-click any program to Launch Now, Cancel Launch, or Stop Application
- Auto-shows when BBS starts (configurable), draggable, always-on-top option
- Position is remembered between sessions
- Access anytime from the system tray right-click menu

### 🔒 Run as Administrator Required

MBBSLauncher now requires administrator privileges for proper operation on modern Windows versions.

**Why This Change?**

A user reported that the launcher did not work correctly on Windows 11 without administrator privileges. Running as administrator ensures:

- ✅ Proper process enumeration and management
- ✅ Ability to bring running BBS applications to foreground
- ✅ Windows startup integration functionality
- ✅ Full access to system-level APIs for process management
- ✅ Reliable operation on Windows 11 and future Windows versions

### 🔍 Auto Launch Duplicate Detection

Auto Launch programs are now checked against running system processes before being started. If a configured program is already running — whether it was launched by MBBSLauncher or started externally — it will be skipped rather than launching a second instance. Skipped launches are recorded in `audit.log`. Manual menu options (1–8) are not affected by this check.

### What You'll See on Launch

When you start MBBSLauncher v1.55, Windows will show a UAC prompt:

```
User Account Control

Do you want to allow this app to make changes to your device?

MBBSLauncher
Verified publisher: Mark Laudenbach

[Yes] [No]
```

Click **Yes** to run the launcher. This is normal and required for proper operation.

## Technical Changes

### Application Manifest
- Added `app.manifest` with `requireAdministrator` execution level
- Windows compatibility declarations for Windows 7-11
- DPI awareness settings for optimal display on high-DPI screens (PerMonitorV2)

### Version Updates
- Updated to version 1.55.0.0 in all file properties
- Updated README.md with all v1.55 feature documentation
- Updated CHANGELOG.md with detailed change history

## Compatibility

- **Operating Systems:** Windows 7, 8, 8.1, 10, 11, Server 2012+
- **Architecture:** 32-bit (x86) - runs on both 32-bit and 64-bit Windows
- **.NET Runtime:** Not required (self-contained build)
- **Permissions:** Administrator privileges required (UAC elevation)

## Upgrade Instructions

1. Download `MBBSLauncher.exe` from the releases page
2. Extract to your existing launcher folder (overwrite old version)
3. Run `MBBSLauncher.exe`
4. Click **Yes** when Windows prompts for administrator permission

Your existing configuration (`MBBSLauncher.ini`) will be preserved. No reconfiguration needed!

## Security Note

**Why is administrator access safe?**

- The application is open source - all code is publicly available for review
- Source code: https://github.com/laudenbachm/MBBS-Launcher
- The UAC prompt ensures you're in control of granting elevated privileges
- Administrator access is only used for legitimate launcher functionality
- No network activity, no data collection, no telemetry

## Known Issues

None reported at this time.

## Getting Help

If you encounter any issues:

- **GitHub Issues:** https://github.com/laudenbachm/MBBS-Launcher/issues
- **Source Code:** https://github.com/laudenbachm/MBBS-Launcher
- **Documentation:** Check the README.md file

## Credits

**Created by:** Mark Laudenbach
**Created with Love in:** Iowa
**License:** MIT

---

## Full Changelog

See [CHANGELOG.md](CHANGELOG.md) for complete version history.

## Previous Versions

- **v1.5** - Self-contained deployment, single instance enforcement, multi-program auto-launch
- **v1.20** - Ghost3 auto-launch support, UI improvements
- **v1.10** - System tray integration, auto-start with Windows
- **v1.00** - Initial release

---

**Thank you for using MBBSLauncher!**

For The Major BBS community - keeping the BBS legacy alive in 2026 and beyond.
