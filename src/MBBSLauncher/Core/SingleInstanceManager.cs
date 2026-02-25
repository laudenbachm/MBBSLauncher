// MBBSLauncher - Single Instance Manager
// Created by Mark Laudenbach with Love in Iowa
// https://github.com/laudenbachm/MBBS-Launcher
//
// File: Core/SingleInstanceManager.cs
// Version: v1.5
//
// Change History:
// 26.02.06.1 - Initial creation for v1.5

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace MBBSLauncher.Core
{
    /// <summary>
    /// Manages single instance enforcement using a named mutex.
    /// Prevents multiple instances of the launcher from running simultaneously.
    /// </summary>
    public static class SingleInstanceManager
    {
        private static Mutex? _instanceMutex;
        private const string MUTEX_NAME = "Global\\MBBSLauncher_SingleInstance_E4F2A1B9";
        private const string WINDOW_TITLE_PREFIX = "MBBSLauncher";

        #region Win32 API Imports

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        #endregion

        /// <summary>
        /// Attempts to acquire the single instance mutex.
        /// Returns true if this is the first instance, false if another instance is already running.
        /// </summary>
        public static bool AcquireInstance()
        {
            try
            {
                _instanceMutex = new Mutex(true, MUTEX_NAME, out bool createdNew);
                return createdNew;
            }
            catch (Exception ex)
            {
                // If mutex creation fails, log and allow instance to run
                // Better to allow duplicate than to block legitimate use
                System.Windows.Forms.MessageBox.Show(
                    $"Warning: Could not check for existing instance.\n\n{ex.Message}\n\nContinuing anyway...",
                    "Single Instance Check Failed",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
                return true; // Allow this instance to run
            }
        }

        /// <summary>
        /// Attempts to restore an existing launcher window to the foreground.
        /// Returns true if an existing window was found and restored.
        /// </summary>
        public static bool RestoreExistingInstance()
        {
            try
            {
                // Try to find existing launcher window by title
                IntPtr hWnd = FindExistingWindow();

                if (hWnd != IntPtr.Zero)
                {
                    // Window found - restore and activate it
                    if (IsIconic(hWnd))
                    {
                        ShowWindow(hWnd, SW_RESTORE);
                    }
                    else
                    {
                        ShowWindow(hWnd, SW_SHOW);
                    }

                    SetForegroundWindow(hWnd);

                    // Flash window to get user's attention
                    FlashWindow(hWnd, true);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Log error but don't prevent new instance from starting
                System.Windows.Forms.MessageBox.Show(
                    $"Could not restore existing window:\n\n{ex.Message}\n\nStarting new instance...",
                    "Restore Failed",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
                return false;
            }
        }

        /// <summary>
        /// Finds the existing launcher window handle.
        /// Searches for windows with titles starting with "MBBSLauncher".
        /// </summary>
        private static IntPtr FindExistingWindow()
        {
            // Try to find by exact version title first
            IntPtr hWnd = FindWindow(null, $"{WINDOW_TITLE_PREFIX} v1.5. Created with Love \u2764 by Mark Laudenbach in Iowa");

            if (hWnd != IntPtr.Zero)
                return hWnd;

            // Try v1.20 title (in case of mixed versions running)
            hWnd = FindWindow(null, $"{WINDOW_TITLE_PREFIX} v1.20. Created with Love \u2764 by Mark Laudenbach in Iowa");

            if (hWnd != IntPtr.Zero)
                return hWnd;

            // Try finding any MBBSLauncher process and get its main window
            var processes = Process.GetProcessesByName("MBBSLauncher");
            if (processes.Length > 0)
            {
                foreach (var process in processes)
                {
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        return process.MainWindowHandle;
                    }
                }
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Releases the single instance mutex.
        /// Should be called when the application exits.
        /// </summary>
        public static void Release()
        {
            try
            {
                if (_instanceMutex != null)
                {
                    _instanceMutex.ReleaseMutex();
                    _instanceMutex.Dispose();
                    _instanceMutex = null;
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw - we're likely shutting down anyway
                Debug.WriteLine($"Error releasing instance mutex: {ex.Message}");
            }
        }
    }
}
